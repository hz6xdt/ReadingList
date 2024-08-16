using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ReadingList.Ui;
using ReadingList.Ui.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("ReadingList.API", httpClient => httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(s => s.GetRequiredService<IHttpClientFactory>().CreateClient("ReadingList.API"));
builder.Services.AddScoped<ILoggingService, ConsoleLoggingService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

await builder.Build().RunAsync();
