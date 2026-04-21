using System.Text.Json.Serialization;

namespace TodoApi.DTOs
{
    public class QuoteDto
    {
        [JsonPropertyName("q")]
        public string Quote { get; set; }

        [JsonPropertyName("a")]
        public string Author { get; set; }
    }
}
