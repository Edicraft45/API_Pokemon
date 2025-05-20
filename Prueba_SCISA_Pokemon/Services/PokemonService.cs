using Newtonsoft.Json;
using Prueba_SCISA_Pokemon.Helpers;
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
            var response = await client.GetAsync("pokemon?limit=100");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);

                foreach (var pokemon in listPokemonsModel.Results)
                {
                    var detailsResponse = await client.GetAsync(pokemon.Url);
                    if (!detailsResponse.IsSuccessStatusCode)
                        continue;

                    var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                    var details = JsonConvert.DeserializeObject<PokemonDetailsResponse>(detailsJson);

                    // Mapea los tipos
                    pokemon.Types = [.. details.Types.Select(t => Enum.TryParse<PokemonType>(Capitalize.Capitalizes(t.Type.Name), true, out var result) ? result : default)];
                }
            }

            return listPokemonsModel;
        }

        public async Task<List<Pokemon>> GetPokemonTypes()
        {
            var result = new List<Pokemon>();

            var client = new HttpClient();
            client.BaseAddress = new Uri(_baseURL);
            var response = await client.GetAsync("type?limit=21");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);

                if (listPokemonsModel.Results != null)
                {
                    result = listPokemonsModel.Results;
                }
            }

            return result;
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
            var response = await client.GetAsync("type?limit=21");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);
            }

            return listPokemonsModel;
        }


    }
}
