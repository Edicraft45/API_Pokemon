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

        /// <summary>
        /// Obtiene los primeros 100 Pokémon desde la PokéAPI, incluyendo sus tipos detallados.
        /// </summary>
        /// <returns>Una instancia de <see cref="ListPokemonsModel"/> con la lista de Pokémon y sus tipos.</returns>
        public async Task<ListPokemonsModel> GetPokemons()
        {
            var listPokemonsModel = new ListPokemonsModel();

            try
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(_baseURL)
                };

                var response = await client.GetAsync("pokemon?limit=100");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    listPokemonsModel = JsonConvert.DeserializeObject<ListPokemonsModel>(json);

                    // Itera sobre cada Pokémon obtenido para obtener sus detalles
                    foreach (var pokemon in listPokemonsModel.Results)
                    {
                        try
                        {
                            // Petición individual al endpoint del Pokémon (para obtener tipos, imagen, etc.)
                            var detailsResponse = await client.GetAsync(pokemon.Url);
                            if (!detailsResponse.IsSuccessStatusCode)
                                continue;

                            var detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                            var details = JsonConvert.DeserializeObject<PokemonDetailsResponse>(detailsJson);

                            // Mapea los tipos obtenidos a enums del tipo PokemonType
                            pokemon.Types = [.. details.Types.Select(t =>
                        Enum.TryParse<PokemonType>(Capitalize.Capitalizes(t.Type.Name), true, out var result)
                            ? result
                            : default
                    )];
                        }
                        catch (HttpRequestException httpEx)
                        {
                            // Log de error de red por cada Pokémon individual (opcional)
                            Console.WriteLine($"Error de red al obtener detalles de {pokemon.Name}: {httpEx.Message}");
                        }
                        catch (JsonException jsonEx)
                        {
                            // Error de deserialización individual
                            Console.WriteLine($"Error al deserializar detalles de {pokemon.Name}: {jsonEx.Message}");
                        }
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error de red al obtener la lista de Pokémon: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al deserializar la lista de Pokémon: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al obtener los Pokémon: {ex.Message}");
            }

            return listPokemonsModel;
        }

        /// <summary>
        /// Obtiene la lista de tipos de Pokémon desde la PokéAPI.
        /// Solo se consideran los primeros 21 tipos (limite establecido).
        /// </summary>
        /// <returns>Una lista de <see cref="PokemonType"/> con los tipos disponibles.</returns>
        public async Task<List<PokemonType>> GetPokemonTypes()
        {
            var types = new List<PokemonType>();

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri(_baseURL);

                var response = await client.GetAsync("type?limit=21");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var typeResponse = JsonConvert.DeserializeObject<TypeListResponse>(json);

                    if (typeResponse?.Results != null)
                    {
                        foreach (var item in typeResponse.Results)
                        {
                            // Intenta mapear el nombre del tipo al enum PokemonType
                            if (Enum.TryParse<PokemonType>(Capitalize.Capitalizes(item.Name), true, out var result))
                            {
                                types.Add(result);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Error al obtener tipos. Código HTTP: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error en la solicitud HTTP: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error al deserializar JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }

            return types;
        }

    }
}
