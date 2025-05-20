using System.Diagnostics;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Index()
        {
            var pokemonList = await _pokemonService.GetPokemons();

            var pokemonTypes = await _pokemonService.GetPokemonTypes();

            PokemonVM = new PokemonVM()
            {
                listPokemonsModel = pokemonList,
                pokemonsType = pokemonTypes
            };
            return View(PokemonVM);
        }
        public async Task<IActionResult> PaginatedPokemons(string url) 
        {
            if (string.IsNullOrWhiteSpace(url)) return RedirectToAction("Index");
            var newPokemons = await _pokemonService.UpdateViewPokemons(url);
            PokemonVM.listPokemonsModel = newPokemons;
            return View("Index", PokemonVM);
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

            for (int i = 0; i < request.Pokemons.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = request.Pokemons[i].Id;
                worksheet.Cell(i + 2, 2).Value = request.Pokemons[i].Name;
                worksheet.Cell(i + 2, 3).Value = request.Pokemons[i].Url;
                worksheet.Cell(i + 2, 4).Value = request.Pokemons[i].ImageUrl;
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
