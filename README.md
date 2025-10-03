# Rick and Morty Character Database

A full-stack .NET 8 application that fetches character and episode data from the Rick and Morty API, stores it in SQL Server, and displays it through a beautiful Blazor WebAssembly frontend with MudBlazor.

## Architecture

This solution follows **Vertical Slice Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                   Blazor WebAssembly (UI)                   │
│              MudBlazor Components + Refit Client            │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ HTTPS/REST API
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Web API                     │
│              Carter (Minimal APIs) + CQRS Pattern           │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│          Entity Framework Core + SQL Server                 │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Domain Models                            │
│              Character, Episode, Location                    │
└─────────────────────────────────────────────────────────────┘

        ┌───────────────────────────────────┐
        │    Console App (Data Seeding)     │
        │   Fetches from Rick & Morty API   │
        └───────────────────────────────────┘
```

### Projects

- **RickAndMorty.Domain** - Domain models (Character, Episode, Location)
- **RickAndMorty.Infrastructure** - EF Core DbContext, configurations, caching
- **RickAndMorty.Console** - Data fetching from Rick and Morty API with upsert logic
- **RickAndMorty.Web** - ASP.NET Core Web API with Carter endpoints
- **RickAndMorty.BlazorWasm** - Blazor WebAssembly UI with MudBlazor
- **RickAndMorty.Tests** - Unit and integration tests

## Tech Stack

### Backend
- **.NET 8** - Latest LTS framework
- **Entity Framework Core 8.0** - ORM with Code First migrations
- **SQL Server 2019** - Database
- **Carter 8.0** - Minimal API endpoints
- **Scrutor 4.2** - Assembly scanning for DI

### Frontend
- **Blazor WebAssembly** - Client-side SPA framework
- **MudBlazor 7.8** - Material Design component library
- **Refit 7.0** - Type-safe HTTP client

### Design Patterns & Principles
- **Vertical Slice Architecture** - Features organized by use case
- **CQRS** - Separate Query and Command handlers
- **Repository Pattern** - Via EF Core DbContext
- **Dependency Injection** - Built-in .NET DI container
- **SOLID Principles** - Single responsibility, dependency inversion
- **YAGNI** - No over-engineering, simple solutions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- SQL Server (via Docker) or local SQL Server instance

## Getting Started

### 1. Start SQL Server Database

**macOS:**
```bash
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=MyS3cr3tP@$$w0rd!' \
  -p 1433:1433 --name RickAndMorty \
  -d mcr.microsoft.com/mssql/server:2019-latest
```

**Windows (PowerShell):**
```powershell
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MyS3cr3tP@$$w0rd!" `
  -p 1433:1433 --name RickAndMorty `
  -d mcr.microsoft.com/mssql/server:2019-latest
```

**Windows (CMD):**
```cmd
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MyS3cr3tP@$$w0rd!" ^
  -p 1433:1433 --name RickAndMorty ^
  -d mcr.microsoft.com/mssql/server:2019-latest
```

**Verify container is running:**
```bash
docker ps
```

**Stop container:**
```bash
docker stop RickAndMorty
```

**Restart container:**
```bash
docker start RickAndMorty
```

**Remove container:**
```bash
docker stop RickAndMorty
docker rm RickAndMorty
```

### 2. Run Console App (Seed Database)

The console application fetches all characters and episodes from the Rick and Morty API and stores them in the database using an upsert strategy.

```bash
cd src/RickAndMorty.Console
dotnet run
```

**What it does:**
- Fetches all characters (alive status only) from https://rickandmortyapi.com/api/character
- Fetches all episodes from https://rickandmortyapi.com/api/episode
- Creates database and applies migrations automatically
- Upserts data (inserts new, updates existing)
- Shows progress: "Successfully processed X characters: Y inserted, Z updated"

### 3. Run Web API

The Web API provides RESTful endpoints for the Blazor frontend.

```bash
cd src/RickAndMorty.Web
dotnet run
```

**API runs at:** `http://localhost:5116` or `https://localhost:7274`

**Swagger UI:** `http://localhost:5116/swagger`

**Available Endpoints:**
- `GET /api/characters` - List all characters (cached 5 minutes)
- `POST /api/characters` - Add new character
- `GET /api/characters/planet/{planetName}` - Filter by planet
- `GET /api/episodes` - List all episodes (cached 5 minutes)

### 4. Run Blazor WebAssembly App

The Blazor app provides a beautiful, responsive UI with Rick and Morty theming.

**Update API URL (if needed):**

Edit `src/RickAndMorty.BlazorWasm/wwwroot/appsettings.json`:
```json
{
  "ApiBaseUrl": "http://localhost:5116"
}
```

**Run the app:**
```bash
cd src/RickAndMorty.BlazorWasm
dotnet run
```

**App runs at:** `http://localhost:5000` or `https://localhost:5001`

## Features

### Console Application
- Fetches data from Rick and Morty public API
- Pagination support (automatically fetches all pages)
- Upsert logic (no duplicate data)
- Progress reporting
- Filters: Only "Alive" characters stored

### Web API
- RESTful endpoints with Carter (Minimal APIs)
- Response caching (5 minutes)
- CORS enabled for Blazor app
- Swagger/OpenAPI documentation
- Vertical slice organization by feature
- Headers: `from-database`, `last-fetched-at`

### Blazor WebAssembly
- **Rick and Morty Theme** - Custom MudBlazor theme with portal green colors
- **Dark/Light Mode** - Toggle between themes
- **Responsive Design** - Mobile, tablet, desktop optimized
- **Characters Page:**
    - Paginated table (10, 25, 50, 100 per page)
    - Search by name or species
    - Filter by planet
    - Filter by status (Alive/Dead/Unknown)
    - Sortable columns
    - Add new characters via modal dialog
- **Planets Page:**
    - Search characters by planet origin
    - Real-time filtering
    - Empty states with helpful messages
- **Episodes Page:**
    - Paginated table with all episodes
    - Search by name or episode code
    - Filter by season (S01, S02, etc.)
    - Sortable columns
    - Air date display
- **Navigation:**
    - Collapsible side drawer
    - Refresh button
    - Theme toggle
    - External API documentation link

### Custom Features
- **Animated Portal Loader** - Rick and Morty themed loading screen
- **Custom Portal Icon** - SVG portal gun icon in app bar
- **Status Color Coding** - Green (alive), Red (dead), Grey (unknown)
- **Form Validation** - Real-time validation with error messages
- **Snackbar Notifications** - Success/error feedback

## Database Schema

### Characters Table
```sql
CREATE TABLE Characters (
    Id INT PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Species NVARCHAR(100) NOT NULL,
    Origin_Name NVARCHAR(200),
    Origin_Url NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL,
    INDEX IX_Characters_Status (Status),
    INDEX IX_Characters_CreatedAt (CreatedAt)
)
```

### Episodes Table
```sql
CREATE TABLE Episodes (
    Id INT PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    EpisodeCode NVARCHAR(10) NOT NULL,
    AirDate NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    INDEX IX_Episodes_EpisodeCode (EpisodeCode),
    INDEX IX_Episodes_CreatedAt (CreatedAt)
)
```

## Development

### Run Tests
```bash
cd src/RickAndMorty.Tests
dotnet test
```

### Create Migration
```bash
cd src/RickAndMorty.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../RickAndMorty.Web
```

### Update Database
```bash
dotnet ef database update --startup-project ../RickAndMorty.Web
```

### Build Solution
```bash
dotnet build
```

### Clean Solution
```bash
dotnet clean
```

## Configuration

### Connection String

Update `appsettings.json` in Console and Web projects:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost,1433;Initial Catalog=RickAndMorty;User ID=sa;Password=MyS3cr3tP@$$w0rd!;Encrypt=True;TrustServerCertificate=True"
  }
}
```

### API Settings

Rick and Morty API configuration in `appsettings.json`:
```json
{
  "RickAndMortyApi": {
    "BaseUrl": "https://rickandmortyapi.com/api",
    "CharacterEndpoint": "/character"
  }
}
```

### CORS Policy

Update allowed origins in `RickAndMorty.Web/Program.cs`:
```csharp
policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
```

## Package Dependencies

### Infrastructure
- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `Microsoft.EntityFrameworkCore.Relational` (8.0.0)

### Web API
- `Carter` (8.0.0) - Minimal API framework
- `Scrutor` (4.2.2) - Assembly scanning
- `Swashbuckle.AspNetCore` (6.6.2) - Swagger/OpenAPI
- `Microsoft.AspNetCore.OpenApi` (8.0.20)

### Console
- `Microsoft.Extensions.Hosting` (8.0.0)
- `Microsoft.Extensions.Http` (8.0.0)

### Blazor WASM
- `Microsoft.AspNetCore.Components.WebAssembly` (8.0.0)
- `MudBlazor` (7.8.0) - UI component library
- `Refit.HttpClientFactory` (7.0.0) - Type-safe HTTP client

### Tests
- `xunit` (2.9.3)
- `Microsoft.NET.Test.Sdk` (18.0.0)
- `Microsoft.EntityFrameworkCore.InMemory` (8.0.0)
- `Moq` (4.20.72)

## Design Patterns Used

### CQRS (Command Query Responsibility Segregation)
- **Queries:** `GetCharactersQuery`, `GetEpisodesQuery`, `GetCharactersByPlanetQuery`
- **Commands:** `AddCharacterCommand`
- Separate read/write concerns

### Repository Pattern
- EF Core `DbContext` acts as repository
- `DbSet<T>` provides generic repository functionality

### Factory Pattern
- `HttpClientFactory` for HTTP clients
- Refit client factory

### Dependency Injection
- Constructor injection throughout
- Scrutor for auto-registration

### Strategy Pattern
- Caching strategy (5-minute cache)
- Filter strategies (status, planet, search)

### Builder Pattern
- `WebApplicationBuilder` for app configuration
- `MudTheme` builder for theming

## API Response Caching

The API implements in-memory caching with `IMemoryCache`:
- **Cache Duration:** 5 minutes
- **Cache Invalidation:** Automatic on POST operations
- **Cache Keys:** Stored in `Infrastructure.Caching.CacheKeys`
- **Headers:** `from-database` indicates cache hit/miss

## Performance Optimizations

1. **Pagination** - MudTable with configurable page sizes
2. **Caching** - Server-side 5-minute cache
3. **Debouncing** - 300ms search debounce
4. **AsNoTracking** - Read-only queries don't track changes
5. **Bulk Operations** - `AddRangeAsync`, `UpdateRange` for batch inserts
6. **Indexing** - Database indexes on Status, CreatedAt, EpisodeCode
7. **Lazy Loading** - Components load data on initialization

## Troubleshooting

### SQL Server Connection Failed
- Ensure Docker container is running: `docker ps`
- Check port 1433 is available: `netstat -an | grep 1433`
- Verify password meets SQL Server complexity requirements

### API CORS Error
- Check allowed origins in Web API CORS policy
- Ensure Blazor app URL matches CORS configuration

### Blazor App Not Loading
- Verify API is running and accessible
- Check `appsettings.json` ApiBaseUrl
- Clear browser cache

### Database Migration Errors
- Delete `Migrations` folder and recreate: `dotnet ef migrations add Initial`
- Drop database and recreate: `docker exec -it RickAndMorty /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'MyS3cr3tP@$$w0rd!' -Q "DROP DATABASE RickAndMorty"`

## License

This project uses data from the [Rick and Morty API](https://rickandmortyapi.com/), which is a free and open API. All Rick and Morty content and images are owned by their respective owners.

## Credits

- **API Data:** [Rick and Morty API](https://rickandmortyapi.com/)
- **UI Framework:** [MudBlazor](https://mudblazor.com/)
- **Theme Inspiration:** Rick and Morty TV Series