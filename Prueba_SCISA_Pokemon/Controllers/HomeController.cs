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


        /// <summary>
        /// Acci�n principal que muestra la lista de Pok�mon con filtros por nombre, tipo y paginaci�n.
        /// Los datos se recuperan desde la sesi�n o desde el servicio si no est�n en cach�.
        /// </summary>
        /// <param name="searchTerm">Nombre del Pok�mon para buscar.</param>
        /// <param name="pokemonType">Tipo de Pok�mon a filtrar (en texto).</param>
        /// <param name="page">N�mero de p�gina para la paginaci�n. Por defecto es 1.</param>
        /// <returns>Vista con el ViewModel de los Pok�mon filtrados y paginados.</returns>
        public async Task<IActionResult> Index(string searchTerm, string pokemonType, int page = 1)
        {
            try
            {
                var session = HttpContext.Session;

                var pokemonList = session.GetObject<ListPokemonsModel>("PokemonList");
                var pokemonTypes = session.GetObject<List<PokemonType>>("PokemonTypes");
                var vm = session.GetObject<PokemonVM>("PokemonVM") ?? new PokemonVM();

                // Si no hay pokemones en sesi�n, se consultan desde el servicio
                if (pokemonList == null)
                {
                    pokemonList = await _pokemonService.GetPokemons();
                    session.SetObject("PokemonList", pokemonList);
                }

                // Si no hay tipos de Pok�mon en sesi�n, se consultan desde el servicio
                if (pokemonTypes == null)
                {
                    pokemonTypes = await _pokemonService.GetPokemonTypes();
                    session.SetObject("PokemonTypes", pokemonTypes);
                }

                // Se comienza con toda la lista y se aplica filtrado progresivo
                var filteredResults = pokemonList.Results.AsQueryable();

                // Convertir el texto del tipo a enum (si es v�lido)
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

                // Paginaci�n
                int pageSize = 10;
                int totalPage = (int)Math.Ceiling((double)filteredResults.Count() / pageSize);

                var paginated = filteredResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Preparar ViewModel
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Index: {ex.Message}");
                return StatusCode(500, "Ocurri� un error al cargar los datos.");
            }
        }

        /// <summary>
        /// Exporta una lista de Pok�mon a un archivo Excel y lo devuelve como descarga al usuario.
        /// </summary>
        /// <param name="request">Objeto que contiene la lista de Pok�mon a exportar.</param>
        /// <returns>Archivo Excel con la informaci�n de los Pok�mon.</returns>
        [HttpPost]
        public IActionResult ExportToExcel(ExportPokemons request)
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Pokemons");

                // Cabeceras
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nombre";
                worksheet.Cell(1, 3).Value = "URL detalles";
                worksheet.Cell(1, 4).Value = "Imagen URL";
                worksheet.Cell(1, 5).Value = "Tipo";

                // Carga de datos fila por fila
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error al exportar a Excel: {ex.Message}");
                return StatusCode(500, "Ocurri� un error al generar el archivo Excel.");
            }
        }

        /// <summary>
        /// Env�a un correo electr�nico con la informaci�n detallada de un Pok�mon especificado por su ID.
        /// </summary>
        /// <param name="n">ID del Pok�mon a enviar por correo.</param>
        /// <returns>Redirecciona a la vista Index, mostrando un mensaje de �xito o error.</returns>
        public async Task<IActionResult> SendPokemonEmail(int n)
        {
            try
            {
                var session = HttpContext.Session;

                // Obtiene la lista de Pok�mon desde la sesi�n
                var pokemons = (session.GetObject<ListPokemonsModel>("PokemonList")?.Results)
                    ?? throw new Exception("La lista de Pok�mon no est� disponible en la sesi�n.");

                // Busca el Pok�mon por su ID
                var pokemon = pokemons.FirstOrDefault(p => p.Id == n);
                if (pokemon == null)
                    return NotFound("Pok�mon no encontrado.");

                // Obtiene credenciales y configuraci�n del correo desde _emailSettings
                var fromEmail = _emailSettings.FromEmail;
                var toEmail = _emailSettings.ToEmail;
                var appPassword = _emailSettings.AppPassword;

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(appPassword))
                    throw new Exception("Faltan credenciales de correo electr�nico.");

                // Construcci�n del cuerpo HTML del correo
                var body = $@"
                            <h2>Datos del Pok�mon</h2>
                            <p><strong>Nombre:</strong> {pokemon.Name}</p>
                            <p><strong>ID:</strong> {pokemon.Id}</p>
                            <p><strong>Tipo:</strong> {string.Join(", ", pokemon.Types ?? [])}</p>
                            <img src='{pokemon.ImageUrl}' alt='{pokemon.Name}' width='150'/>
                        ";

                // Configura el cliente SMTP
                using var smtp = new SmtpClient(_emailSettings.SmtpHost)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true
                };

                // Crea el mensaje a enviar
                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = $"Datos del Pok�mon: {pokemon.Name}",
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                // Env�o del correo
                await smtp.SendMailAsync(message);

                // Mensaje temporal de �xito para mostrar en la vista
                TempData["SuccessMessage"] = $"Correo enviado correctamente con la informaci�n de {pokemon.Name}.";
            }
            catch (SmtpException smtpEx)
            {
                TempData["ErrorMessage"] = $"Error SMTP al enviar el correo: {smtpEx.Message}";
            }
            catch (FormatException formatEx)
            {
                TempData["ErrorMessage"] = $"Direcci�n de correo inv�lida: {formatEx.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retorna los detalles de un Pok�mon espec�fico, combinando informaci�n de la Pok�API
        /// con los tipos almacenados previamente en sesi�n.
        /// </summary>
        /// <param name="n">El ID del Pok�mon.</param>
        /// <returns>
        /// Un objeto JSON que contiene la informaci�n b�sica del Pok�mon junto con sus tipos.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetPokemon(int n)
        {
            try
            {
                // Obtener detalles desde la API
                var details = await _pokemonService.GetPokemonDetails(n);
                if (details == null)
                    return NotFound("No se encontraron detalles del Pok�mon.");

                // Obtener tipos desde la sesi�n
                var pokemons = HttpContext.Session.GetObject<ListPokemonsModel>("PokemonList")?.Results;
                var pokemon = pokemons?.FirstOrDefault(p => p.Id == n);
                var types = pokemon?.Types?.Select(t => t.ToString()) ?? new List<string>();

                // Devolver JSON con detalles + tipos
                return Json(new
                {
                    details.Id,
                    details.Name,
                    details.Base_happiness,
                    details.Capture_rate,
                    details.Is_legendary,
                    details.Is_mythical,
                    details.ImageUrl,
                    Types = types
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalles del Pok�mon: {ex.Message}");
                return StatusCode(500, "Ocurri� un error al obtener los detalles del Pok�mon.");
            }
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
