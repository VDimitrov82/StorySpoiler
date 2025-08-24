using System.Text.Json.Serialization;

namespace StorySpoil.Models
{
    
    internal class StoryDTO
    {
        [JsonPropertyName("title")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
