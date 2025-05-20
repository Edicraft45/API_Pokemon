namespace Prueba_SCISA_Pokemon.Helpers
{
    public class Capitalize
    {
        public static string Capitalizes(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }

    }
}
