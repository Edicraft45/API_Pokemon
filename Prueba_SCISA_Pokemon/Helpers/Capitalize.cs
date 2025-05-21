namespace Prueba_SCISA_Pokemon.Helpers
{
    public class Capitalize
    {
        /// <summary>
        /// Convierte la primera letra de una cadena a mayúscula y el resto a minúscula.
        /// </summary>
        /// <param name="value">Cadena de texto que se desea capitalizar.</param>
        /// <returns>
        /// Una nueva cadena con la primera letra en mayúscula y el resto en minúsculas.
        /// Si la cadena es nula o vacía, se devuelve tal cual.
        /// </returns>
        public static string Capitalizes(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }


    }
}
