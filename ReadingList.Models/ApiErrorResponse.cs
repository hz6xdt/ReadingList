using System.Text.Json.Serialization;

namespace ReadingList.Models
{
    public class ApiErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public IEnumerable<string>? Errors { get; set; }
    }
}
