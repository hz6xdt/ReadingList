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
    }
}
