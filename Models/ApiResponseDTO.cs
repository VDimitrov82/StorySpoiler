using System.Text.Json.Serialization;

namespace StorySpoil.Models
{
    internal class ApiResponseDTO
    {
        
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyid")]
        public string? FoodId { get; set; }
    }
}
