using Prueba_SCISA_Pokemon.Helpers;
using Prueba_SCISA_Pokemon.Models;

namespace Prueba_SCISA_Pokemon.Services
{
    public interface IPokemonService
    {
        Task<ListPokemonsModel> GetPokemons();
        Task<List<PokemonType>> GetPokemonTypes();
    }
}