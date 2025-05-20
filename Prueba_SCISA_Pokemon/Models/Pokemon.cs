namespace Prueba_SCISA_Pokemon.Models
{
    public class Pokemon
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public int Id
        {
            get
            {
                var parts = Url.TrimEnd('/').Split('/');
                return int.Parse(parts[^1]);
            }
        }

        public string ImageUrl => $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/{Id}.png";
    }
}
