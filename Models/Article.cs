using System;
using System.Text.Json.Serialization;

namespace NewsReaderHTTP.Models
{
    public class Article
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("publishedAt")]
        public DateTime PublishedAt { get; set; }
    }
}
