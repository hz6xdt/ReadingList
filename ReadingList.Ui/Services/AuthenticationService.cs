using ReadingList.Models;
using ReadingList.Ui.Exceptions;
using System.Net.Http.Json;

namespace ReadingList.Ui.Services
{
    public class AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        public async Task<LoginResponse> LoginUserAsync(LoginRequest requestModel)
        {
            var response = await httpClient.PostAsJsonAsync("/api/account/token", requestModel);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>() ?? new();
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ApiResponseException(error ?? new());
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to log the user in.  Status code: {response.StatusCode} -- {content}", response.StatusCode, content);
                    throw new Exception("Oops! Something went wrong...");
                }
            }
        }

        public async Task<LoginResponse> LogoutUserAsync()
        {
            var response = await httpClient.PostAsJsonAsync("/api/account/logout", new LoginRequest());

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>() ?? new();
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                    throw new ApiResponseException(error ?? new());
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    logger.LogError("Failed to log the user out.  Status code: {response.StatusCode} -- {content}", response.StatusCode, content);
                    throw new Exception("Oops! Something went wrong...");
                }
            }
        }

        public async Task RegisterUserAsync(RegisterUserRequest registerModel)
        {
            var response = await httpClient.PostAsJsonAsync("/api/account/register", registerModel);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                throw new ApiResponseException(error ?? new());
            }
            else
            {
                throw new Exception("Oops! Something went wrong...");
            }
        }
    }
}
