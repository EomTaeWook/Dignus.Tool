using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared
{
    public class JsonHelper
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
        };
        static JsonHelper()
        {
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }
    }
}