using ReadingList.Models;

namespace ReadingList.Ui.Services;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginUserAsync(LoginRequest requestModel);
    Task<LoginResponse> LogoutUserAsync();
    Task RegisterUserAsync(RegisterUserRequest registerModel);
}
