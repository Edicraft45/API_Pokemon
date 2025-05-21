using Prueba_SCISA_Pokemon.Helpers;
using Prueba_SCISA_Pokemon.Models;

namespace Prueba_SCISA_Pokemon.Services
{
    public interface IPokemonService
    {
        /// <summary>
        /// Obtiene los primeros 100 Pokémon desde la PokéAPI, incluyendo sus tipos detallados.
        /// </summary>
        /// <returns>Una instancia de <see cref="ListPokemonsModel"/> con la lista de Pokémon y sus tipos.</returns>
        Task<ListPokemonsModel> GetPokemons();

        /// <summary>
        /// Obtiene la lista de tipos de Pokémon desde la PokéAPI.
        /// Solo se consideran los primeros 21 tipos (limite establecido).
        /// </summary>
        /// <returns>Una lista de <see cref="PokemonType"/> con los tipos disponibles.</returns>
        Task<List<PokemonType>> GetPokemonTypes();

        /// <summary>
        /// Obtiene los detalles adicionales de un Pokémon desde la PokéAPI
        /// </summary>
        /// <param name="id">El ID del Pokémon a consultar.</param>
        /// <returns>
        /// Un objeto <see cref="PokemonDetails"/> que contiene información como felicidad base,
        /// tasa de captura, si es legendario o mítico, etc.
        /// </returns>
        Task<PokemonDetails> GetPokemonDetails(int id);
    }
}