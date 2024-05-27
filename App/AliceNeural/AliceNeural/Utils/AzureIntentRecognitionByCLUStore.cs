using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Utils
{
    public class AzureIntentRecognitionByCLUStore
    {
        [JsonPropertyName("api_key")]
        public string KeyValue { get; set; } = string.Empty;

        [JsonPropertyName("location_region")]
        public string LanguageLocationRegion { get; set; } = string.Empty;

        [JsonPropertyName("language_endpoint")]
        public string LanguageEndpoint { get; set; } = string.Empty;

    }
}
