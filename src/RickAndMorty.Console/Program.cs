using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RickAndMorty.Console.Features;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.Configure<RickAndMortyApiSettings>(
    builder.Configuration.GetSection(RickAndMortyApiSettings.SectionName));

builder.Services.AddHttpClient<IFetchCharactersService, FetchCharactersService>();
builder.Services.AddHttpClient<IFetchEpisodesService, FetchEpisodesService>();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo<IFetchCharactersService>())
    .AsImplementedInterfaces()
    .WithTransientLifetime()
);

var host = builder.Build();

using var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync();

var fetchService = scope.ServiceProvider.GetRequiredService<IFetchCharactersService>();
await fetchService.ExecuteAsync();

var fetchEpisodesService = scope.ServiceProvider.GetRequiredService<IFetchEpisodesService>();
await fetchEpisodesService.ExecuteAsync();

System.Console.WriteLine("Press any key to exit...");
System.Console.ReadKey();