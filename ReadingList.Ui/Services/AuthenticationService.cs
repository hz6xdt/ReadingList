using ReadingList.Models;
using System.Net.Http.Json;

namespace ReadingList.Ui.Services
{
    public class AuthenticationService(HttpClient httpClient) : IAuthenticationService
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
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                Console.WriteLine(error);
                throw new Exception(error?.Message);
            }
        }
    }
}
