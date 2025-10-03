namespace RickAndMorty.Infrastructure.Configuration;

public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = 
        "Server=localhost,1433;Database=RickAndMorty;User Id=sa;Password=RickAndMorty123!;TrustServerCertificate=True;Encrypt=False";
}