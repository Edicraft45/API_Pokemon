using Newtonsoft.Json;
using Prueba_SCISA_Pokemon.Models;

namespace Prueba_SCISA_Pokemon.Services
{
    public class PokemonService : IPokemonService
    {
        private static string? _baseURL;
        public PokemonService()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            _baseURL = builder.GetSection("PokemonAPI:BaseUrl").Value;
        }

        public async Task<ListPokemonsModel> GetPokemons()
        {
            var listPokemonsModel = new ListPokemonsModel();

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            var response = await client.GetAsync("pokemon?limit=50");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);
            }

            return listPokemonsModel;
        }

        public async Task<ListPokemonsModel> UpdateViewPokemons(string url)
        {
            var listPokemonsModel = new ListPokemonsModel();

            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);
            }

            return listPokemonsModel;
        }

        public async Task<ListPokemonsModel> GetPokemonsByName(string name)
        {
            var listPokemonsModel = new ListPokemonsModel();

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            var response = await client.GetAsync("pokemon?limit=50");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);
            }

            return listPokemonsModel;
        }
    }
}
