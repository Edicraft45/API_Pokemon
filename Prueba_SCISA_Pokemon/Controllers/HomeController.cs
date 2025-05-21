using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Prueba_SCISA_Pokemon.Helpers;
using Prueba_SCISA_Pokemon.Models;
using Prueba_SCISA_Pokemon.Services;
using Microsoft.Extensions.Options;

namespace Prueba_SCISA_Pokemon.Controllers
{
    public class HomeController : Controller
    {
        private static IPokemonService _pokemonService;
        private readonly EmailSettings _emailSettings;
        public HomeController(IPokemonService pokemonService, IOptions<EmailSettings> emailOptions)
        {
            _pokemonService = pokemonService;
            _emailSettings = emailOptions.Value;
        }


        public async Task<IActionResult> Index(string searchTerm, string pokemonType, int page = 1)
        {
            var session = HttpContext.Session;

            var pokemonList = session.GetObject<ListPokemonsModel>("PokemonList");
            var pokemonTypes = session.GetObject<List<PokemonType>>("PokemonTypes");
            var vm = session.GetObject<PokemonVM>("PokemonVM") ?? new PokemonVM();

            if (pokemonList == null)
            {
                pokemonList = await _pokemonService.GetPokemons();
                session.SetObject("PokemonList", pokemonList);
            }

            if (pokemonTypes == null)
            {
                pokemonTypes = await _pokemonService.GetPokemonTypes();
                session.SetObject("PokemonTypes", pokemonTypes);
            }

            var filteredResults = pokemonList.Results.AsQueryable();

            // Convertir tipo string a enum (si aplica)
            PokemonType? filterType = null;
            if (!string.IsNullOrEmpty(pokemonType) &&
                Enum.TryParse<PokemonType>(pokemonType, true, out var parsedType))
            {
                filterType = parsedType;
            }

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredResults = filteredResults
                    .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por tipo
            if (filterType.HasValue)
            {
                filteredResults = filteredResults
                    .Where(p => p.Types != null && p.Types.Contains(filterType.Value));
            }

            // Paginación
            int pageSize = 10;
            int totalPage = (int)Math.Ceiling((double)filteredResults.Count() / pageSize);

            var paginated = filteredResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Asignar valores al ViewModel
            vm.listPokemonsModel = new ListPokemonsModel
            {
                Count = filteredResults.Count(),
                Results = paginated
            };
            vm.pokemonsType = pokemonTypes;
            vm.TotalPages = totalPage;
            vm.CurrentPage = page;
            vm.SearchTerm = searchTerm;
            vm.SearchType = pokemonType;

            return View(vm);
        }

        [HttpPost]
        public IActionResult ExportToExcel(ExportPokemons request)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Pokemons");

            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "URL detalles";
            worksheet.Cell(1, 4).Value = "Imagen URL";
            worksheet.Cell(1, 5).Value = "Tipo";

            for (int i = 0; i < request.Pokemons.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = request.Pokemons[i].Id;
                worksheet.Cell(i + 2, 2).Value = request.Pokemons[i].Name;
                worksheet.Cell(i + 2, 3).Value = request.Pokemons[i].Url;
                worksheet.Cell(i + 2, 4).Value = request.Pokemons[i].ImageUrl;
                worksheet.Cell(i + 2, 5).Value = request.Pokemons[i].TypesString;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "pokemons.xlsx");
        }

        public async Task<IActionResult> SendPokemonEmail(int n)
        {
            try
            {
                var session = HttpContext.Session;
                var pokemons = (session.GetObject<ListPokemonsModel>("PokemonList")?.Results) ?? throw new Exception("La lista de Pokémon no está disponible en la sesión.");
                var pokemon = pokemons.FirstOrDefault(p => p.Id == n);
                if (pokemon == null)
                    return NotFound("Pokémon no encontrado.");

                var fromEmail = _emailSettings.FromEmail;
                var toEmail = _emailSettings.ToEmail;
                var appPassword = _emailSettings.AppPassword;

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(appPassword))
                    throw new Exception("Faltan credenciales de correo electrónico.");

                var body = $@"
                                <h2>Datos del Pokémon</h2>
                                <p><strong>Nombre:</strong> {pokemon.Name}</p>
                                <p><strong>ID:</strong> {pokemon.Id}</p>
                                <p><strong>Tipo:</strong> {string.Join(", ", pokemon.Types ?? [])}</p>
                                <img src='{pokemon.ImageUrl}' alt='{pokemon.Name}' width='150'/>
                            ";

                using var smtp = new SmtpClient(_emailSettings.SmtpHost)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = $"Datos del Pokémon: {pokemon.Name}",
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await smtp.SendMailAsync(message);

                TempData["SuccessMessage"] = $"Correo enviado correctamente con la información de {pokemon.Name}.";
            }
            catch (SmtpException smtpEx)
            {
                TempData["ErrorMessage"] = $"Error SMTP al enviar el correo: {smtpEx.Message}";
            }
            catch (FormatException formatEx)
            {
                TempData["ErrorMessage"] = $"Dirección de correo inválida: {formatEx.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToAction("Index");
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
