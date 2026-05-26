using System.Text.Json;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Single source of truth for JsonSerializerOptions across the data layer.
    /// All deserialization must go through GetOptions() to stay consistent.
    /// </summary>
    public static class BattlemancersJsonHelper
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static JsonSerializerOptions GetOptions() => _options;

        public static T Deserialize<T>(string json) =>
            JsonSerializer.Deserialize<T>(json, _options);

        public static T DeserializeFile<T>(string filePath) =>
            Deserialize<T>(System.IO.File.ReadAllText(filePath));
    }
}
