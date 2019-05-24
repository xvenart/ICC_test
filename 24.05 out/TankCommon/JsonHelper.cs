using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace TankCommon
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto };

        /// <summary>
        /// Конвертировать в JSON.
        /// </summary>
        /// <param name="obj">Объект параметра.</param>
        public static string ToJson(this object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented, Settings);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Конвертировать из JSON в <typeparam name="T">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">Результирующий тип.</typeparam>
        /// <param name="jsonString">Исходный JSON.</param>
        /// <returns></returns>
        public static T FromJson<T>(this string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString, Settings);
            }
            catch
            {
                return default(T);
            }
        }
    }
}
