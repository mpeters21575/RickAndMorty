using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Refit;
using RickAndMorty.BlazorWasm;
using RickAndMorty.BlazorWasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5116";

builder.Services.AddRefitClient<ICharacterApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});

builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

await builder.Build().RunAsync();