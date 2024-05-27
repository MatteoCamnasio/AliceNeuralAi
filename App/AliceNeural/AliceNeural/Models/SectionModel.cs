using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AliceNeural.Models
{
    public class Parse
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("pageid")]
        public int? Pageid { get; set; }

        [JsonPropertyName("sections")]
        public List<SectionWiki?>? Sections { get; set; }

        [JsonPropertyName("showtoc")]
        public string? Showtoc { get; set; }
    }

    public class SectionModel
    {
        [JsonPropertyName("parse")]
        public Parse? Parse { get; set; }
    }

    public class SectionWiki
    {
        [JsonPropertyName("toclevel")]
        public int? Toclevel { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("line")]
        public string? Line { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("index")]
        public string? Index { get; set; }

        [JsonPropertyName("fromtitle")]
        public string? Fromtitle { get; set; }

        [JsonPropertyName("byteoffset")]
        public int? Byteoffset { get; set; }

        [JsonPropertyName("anchor")]
        public string? Anchor { get; set; }

        [JsonPropertyName("linkAnchor")]
        public string? LinkAnchor { get; set; }
    }
}
