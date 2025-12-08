using Blazored.LocalStorage;

namespace ReadingList.Ui.Services;

public class AuthorizationMessageHandler(ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (await localStorage.ContainKeyAsync("access_token"))
        {
            var token = await localStorage.GetItemAsync<string>("access_token");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
