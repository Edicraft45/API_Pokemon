using Newtonsoft.Json;

namespace Prueba_SCISA_Pokemon.Helpers
{
    /// <summary>
    /// Extensiones para guardar y recuperar objetos complejos en la sesión HTTP serializándolos en JSON.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Serializa un objeto a JSON y lo guarda en la sesión bajo la clave especificada.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a guardar.</typeparam>
        /// <param name="session">La sesión HTTP donde se guardará el objeto.</param>
        /// <param name="key">Clave bajo la cual se almacenará el objeto.</param>
        /// <param name="value">El objeto a almacenar en sesión.</param>
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            session.SetString(key, json);
        }

        /// <summary>
        /// Recupera un objeto serializado en JSON de la sesión y lo deserializa al tipo especificado.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a recuperar.</typeparam>
        /// <param name="session">La sesión HTTP desde donde se recuperará el objeto.</param>
        /// <param name="key">Clave bajo la cual se almacenó el objeto.</param>
        /// <returns>El objeto deserializado o el valor predeterminado de T si no se encuentra.</returns>
        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default : JsonConvert.DeserializeObject<T>(json);
        }
    }


}
