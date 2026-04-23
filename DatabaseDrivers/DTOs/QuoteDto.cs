using System.Text.Json.Serialization;

namespace TodoApi.DTOs
{
    public class QuoteDto
    {
        [JsonPropertyName("q")]
        public required string Quote { get; set; }

        [JsonPropertyName("a")]
        public required string Author { get; set; }
    }
}
