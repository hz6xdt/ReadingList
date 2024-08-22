using System.ComponentModel.DataAnnotations;

public class RegisterUserRequest
{
    [StringLength(256, MinimumLength = 12)]
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }

    [StringLength(256)]
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }

    [StringLength(256, MinimumLength = 6)]
    public required string Email { get; set; }
}
