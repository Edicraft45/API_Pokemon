using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Prueba_SCISA_Pokemon.Helpers;
using Prueba_SCISA_Pokemon.Models;
using Prueba_SCISA_Pokemon.Services;

namespace Prueba_SCISA_Pokemon.Controllers
{
    public class HomeController : Controller
    {
        private static IPokemonService _pokemonService;
        private static PokemonVM PokemonVM;
        public HomeController(IPokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }


        public async Task<IActionResult> Index(string searchTerm, int page = 1, PokemonType? pokemonType = null)
        {
            var session = HttpContext.Session;

            var pokemonList = session.GetObject<ListPokemonsModel>("PokemonList");
            var pokemonTypes = session.GetObject<List<PokemonType>>("PokemonTypes");

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

            //Lista Filtrada
            var filteredResults = pokemonList.Results;

            // Filtro por nombre (si hay)
            if (!string.IsNullOrWhiteSpace(searchTerm)) filteredResults = [.. filteredResults.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))];

            // Filtro por tipo (si hay)
            if (pokemonType.HasValue) filteredResults = [.. filteredResults.Where(p => p.Types != null && p.Types.Contains(pokemonType.Value))];


            // Paginación
            int pageSize = 10;
            var totalPage = (int)Math.Ceiling((double)(pokemonList.Results.Count) / pageSize);
            var paginated = filteredResults
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new PokemonVM
            {
                listPokemonsModel = new ListPokemonsModel
                {
                    Count = filteredResults.Count,
                    Results = paginated
                },
                pokemonsType = pokemonTypes,
                TotalPages = totalPage,
                CurrentPage = page,
                SearchTerm = searchTerm
            };

            return View(vm);
        }
        public async Task<IActionResult> PaginatedPokemons(int page = 1) 
        {
            PokemonVM.CurrentPage = page;
            return View("Index", PokemonVM);
            //if (string.IsNullOrWhiteSpace(url)) return RedirectToAction("Index");
            //var newPokemons = await _pokemonService.UpdateViewPokemons(url);
            //PokemonVM.listPokemonsModel = newPokemons;
            //return View("Index", PokemonVM);
        }
        public IActionResult FilerByName(string name)
        {

            return View();
        }
        public IActionResult FilterByType(PokemonType type)
        {
            return View(type);
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
