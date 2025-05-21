using Prueba_SCISA_Pokemon.Helpers;

namespace Prueba_SCISA_Pokemon.Models
{
    public class PokemonDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Base_happiness { get; set; }
        public int Capture_rate { get; set; }
        public bool Is_legendary { get; set; }
        public bool Is_mythical { get; set; }
        public PokemonType PokemonType { get; set; }
        public string ImageUrl => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{Id}.png";
    }
}
