using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReadingList.Ui.Services
{
    public class JwtAuthenticationStateProvider(ILocalStorageService localStorage) : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (await localStorage.ContainKeyAsync("access_token"))
            {
                var tokenAsString = await localStorage.GetItemAsync<string>("access_token");
                var tokenHandler = new JwtSecurityTokenHandler();

                var token = tokenHandler.ReadJwtToken(tokenAsString);

                var expires = token.ValidTo;

                if (expires.CompareTo(DateTime.UtcNow) > 0)
                {
                    var identity = new ClaimsIdentity(token.Claims, "Bearer");
                    var user = new ClaimsPrincipal(identity);
                    var authState = new AuthenticationState(user);

                    NotifyAuthenticationStateChanged(Task.FromResult(authState));

                    return authState;
                }
            }

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());  // No claims or authentication scheme provided
            var anonymousAuthState = new AuthenticationState(anonymousUser);

            NotifyAuthenticationStateChanged(Task.FromResult(anonymousAuthState));

            return anonymousAuthState;
        }
    }
}
