﻿using Prueba_SCISA_Pokemon.Helpers;

namespace Prueba_SCISA_Pokemon.Models
{
    public class PokemonVM
    {
        public ListPokemonsModel listPokemonsModel { get; set; } = new ListPokemonsModel();
        public List<PokemonType> pokemonsType { get; set; } = new List<PokemonType>();

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchType { get; set; } = string.Empty;

        public int TotalPages { get; set; }

        public List<Pokemon>? PagedPokemons =>
            listPokemonsModel?.Results
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
    }
}
