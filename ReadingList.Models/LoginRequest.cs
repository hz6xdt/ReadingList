using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ReadingList.Models
{
    public class LoginRequest
    {
        [Required]
        [JsonPropertyName("username")]
        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("password")]
        [StringLength(256, MinimumLength = 12)]
        public string Password { get; set; } = string.Empty;
    }
}
