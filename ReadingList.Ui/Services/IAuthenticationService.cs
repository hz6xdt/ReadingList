using ReadingList.Models;

namespace ReadingList.Ui.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginUserAsync(LoginRequest requestModel);
    }
}
