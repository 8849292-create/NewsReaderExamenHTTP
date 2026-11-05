using NewsReaderHTTP.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NewsReaderExamenHTTP.Models
{
    public class NewsApiResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("articles")]
        public List<Article> Articles { get; set; }= new List<Article>();
    }
}
