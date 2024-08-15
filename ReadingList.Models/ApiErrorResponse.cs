using System.Text.Json.Serialization;

namespace ReadingList.Models
{
    public class ApiErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public string[]? Errors { get; set; }
    }
}
