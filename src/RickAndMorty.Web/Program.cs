using Carter;
using Microsoft.EntityFrameworkCore;
using RickAndMorty.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("from-database", "last-fetched-at");
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Rick and Morty API",
        Version = "v1",
        Description = "API for managing Rick and Morty characters"
    });
});

builder.Services.AddCarter();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(type => 
        type.Name.EndsWith("Query") || 
        type.Name.EndsWith("Command")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rick and Morty API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("AllowBlazor");

app.MapCarter();

await app.RunAsync();