using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prueba_SCISA_Pokemon.Models;
using Prueba_SCISA_Pokemon.Services;

namespace Prueba_SCISA_Pokemon.Controllers
{
    public class HomeController : Controller
    {
        private static IPokemonService _pokemonService;

        public HomeController(IPokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        public async Task<IActionResult> Index()
        {
            var pokemonList = await _pokemonService.GetPokemons();
            return View(pokemonList);
        }
        public async Task<IActionResult> PaginatedPokemons(string url) 
        {
            if (string.IsNullOrWhiteSpace(url)) return RedirectToAction("Index");
            var newPokemons = await _pokemonService.UpdateViewPokemons(url);

            return View("Index", newPokemons);
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
