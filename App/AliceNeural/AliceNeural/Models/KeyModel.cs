using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Models
{
    public class Page
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("excerpt")]
        public string Excerpt { get; set; }

        [JsonPropertyName("matched_title")]
        public object MatchedTitle { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("thumbnail")]
        public Thumbnail Thumbnail { get; set; }
    }

    public class KeyModel
    {
        [JsonPropertyName("pages")]
        public List<Page> Pages { get; set; }
    }

    public class Thumbnail
    {
        [JsonPropertyName("mimetype")]
        public string Mimetype { get; set; }

        [JsonPropertyName("size")]
        public object Size { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("duration")]
        public object Duration { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
