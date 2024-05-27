using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Models
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Parses
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("pageid")]
        public int Pageid { get; set; }

        [JsonPropertyName("wikitext")]
        public Wikitext Wikitext { get; set; }
    }

    public class WikiSection
    {
        [JsonPropertyName("parse")]
        public Parses Parses { get; set; }
    }

    public class Wikitext
    {
        [JsonPropertyName("*")]
        public string Risposta { get; set; }
}


}
