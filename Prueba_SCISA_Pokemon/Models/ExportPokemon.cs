namespace Prueba_SCISA_Pokemon.Models
{
    public class ExportPokemon
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string TypesString { get; set; } // ← Aquí se recibirá la cadena "Fire,Poison"
        public string ImageUrl { get; set; }
        public int Id { get; set; }

        
    }

}
