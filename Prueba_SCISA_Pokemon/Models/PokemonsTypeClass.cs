using Newtonsoft.Json;

namespace Prueba_SCISA_Pokemon.Models
{
    public class PokemonDetailsResponse
    {
        [JsonProperty("types")]
        public List<TypeSlot> Types { get; set; }
    }

    public class TypeSlot
    {

        [JsonProperty("type")]
        public NamedApiResource Type { get; set; }
    }

    public class NamedApiResource
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class TypeListResponse
    {
        public List<TypeResult> Results { get; set; }
    }

    public class TypeResult
    {
        public string Name { get; set; }
    }


}
