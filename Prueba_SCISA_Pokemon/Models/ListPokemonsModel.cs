namespace Prueba_SCISA_Pokemon.Models
{
    public class ListPokemonsModel
    {
        public int Count { get; set; }
        public string Next { get; set; } = string.Empty;
        public string Previous { get; set; } = string.Empty;
        public List<Pokemon> Results { get; set; }
    }
}
