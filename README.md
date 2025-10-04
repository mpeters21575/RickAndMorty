# Rick and Morty Character Database

A full-stack .NET 8 application for managing Rick and Morty characters with real-time notifications using SignalR. The application features a Blazor WebAssembly frontend, ASP.NET Core Web API backend, and background services for monitoring database changes.

## Architecture

### Clean Architecture Pattern
- **Domain Layer** - Core entities and business logic
- **Infrastructure Layer** - Data access with EF Core, configurations
- **Application Layer** - Web API with vertical slice architecture
- **Presentation Layer** - Blazor WebAssembly SPA

### Projects Structure

```
RickAndMorty/
├── RickAndMorty.Domain/              # Core entities
├── RickAndMorty.Infrastructure/      # EF Core, DbContext, configurations
├── RickAndMorty.Console/             # Data seeding CLI app
├── RickAndMorty.Web/                 # ASP.NET Core Web API
├── RickAndMorty.BlazorWasm/          # Blazor WASM frontend
└── RickAndMorty.Tests/               # Unit & integration tests
```

## Features

### Backend (Web API)
- **Vertical Slice Architecture** with Carter for minimal APIs
- **CQRS Pattern** - Separate read (Query) and write (Command) operations
- **SignalR Hub** - Real-time push notifications to clients
- **Background Service** - Monitors database for new characters
- **Memory Caching** - 5-minute cache for character and episode data
- **Entity Framework Core** - SQL Server database with migrations
- **Swagger/OpenAPI** - API documentation

### Frontend (Blazor WASM)
- **MudBlazor UI** - Modern Material Design components
- **Real-time Notifications** - Bell icon with unread badge
- **SignalR Client** - Receives live updates from API
- **Refit** - Type-safe HTTP client
- **Responsive Design** - Dark/light theme support
- **Advanced Filtering** - Search by name, status, species, and planet

### Key Capabilities
- View all Rick and Morty characters
- Filter characters by name, status, and species
- Search characters by origin planet
- Browse all episodes with character counts
- Add custom characters to the database
- Real-time notifications when new characters are added
- Background monitoring of database changes
- Automatic cache invalidation

## Technology Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server
- Carter (Minimal APIs)
- SignalR
- Scrutor (Assembly scanning)
- Swashbuckle (Swagger)

### Frontend
- Blazor WebAssembly (.NET 8)
- MudBlazor 7.8.0
- Refit 8.0 (HTTP client)
- SignalR Client 7.0

### Testing
- xUnit
- Moq
- EF Core InMemory

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, Docker, or Azure SQL)
- Docker (optional - for SQL Server container)
- Visual Studio 2022 or VS Code

### Database Setup

#### Option 1: Using Docker (Recommended)

Run SQL Server in a Docker container:

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YOUR_STRONG_PWD' -p 1433:1433 --name RickAndMorty -d mcr.microsoft.com/mssql/server:2019-latest
```

Connection string is already configured in `appsettings.json` to use this container.

#### Option 2: Using Local SQL Server

Update the connection string in `RickAndMorty.Web/appsettings.json` to point to your SQL Server instance.

#### Database Migrations

Migrations are **automatically applied** when the Web API starts. Alternatively, apply manually:

```bash
cd RickAndMorty.Web
dotnet ef database update
```

#### Seed Initial Data

Run the Console app to fetch and populate data from the Rick and Morty API:

```bash
cd RickAndMorty.Console
dotnet run
```

This fetches all characters and episodes from the public API and stores them in your database.

### Running the Application

#### Option 1: Multiple Startup Projects (Visual Studio)

1. Right-click solution → Properties → Multiple Startup Projects
2. Set both `RickAndMorty.Web` and `RickAndMorty.BlazorWasm` to **Start**
3. Press F5

#### Option 2: Command Line

Terminal 1 - API:
```bash
cd RickAndMorty.Web
dotnet run
```

Terminal 2 - Blazor WASM:
```bash
cd RickAndMorty.BlazorWasm
dotnet run
```

## Configuration

### Web API Settings (`RickAndMorty.Web/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RickAndMorty;User ID=sa;Password=YOUR_STRONG_PWD;TrustServerCertificate=True;Encrypt=False"
  },
  "RickAndMortyApi": {
    "BaseUrl": "https://rickandmortyapi.com/api",
    "CharacterEndpoint": "/character"
  },
  "CharacterMonitor": {
    "Enabled": true,
    "IntervalMinutes": 5,
    "TestMode": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Connection String Settings
| Setting | Description | Default |
|---------|-------------|---------|
| `Server` | SQL Server host and port | `localhost,1433` |
| `Database` | Database name | `RickAndMorty` |
| `User ID` | SQL Server username | `sa` |
| `Password` | SQL Server password | `YOUR_STRONG_PWD` |
| `TrustServerCertificate` | Trust self-signed certificates | `True` |
| `Encrypt` | Use encrypted connection | `False` |

#### Rick and Morty API Settings
| Setting | Description | Default |
|---------|-------------|---------|
| `BaseUrl` | External API base URL | `https://rickandmortyapi.com/api` |
| `CharacterEndpoint` | Character endpoint path | `/character` |

#### Character Monitor Settings
| Setting | Description | Default | Values |
|---------|-------------|---------|--------|
| `Enabled` | Enable background monitoring | `true` | `true`/`false` |
| `IntervalMinutes` | Check interval in minutes | `5` | Any positive integer |
| `TestMode` | Send test messages instead of real notifications | `false` | `true`/`false` |

**Note:** When `TestMode = true`, the monitor sends periodic test messages to verify SignalR connectivity without checking for actual database changes.

### Blazor WASM Settings (`RickAndMorty.BlazorWasm/wwwroot/appsettings.json`)

```json
{
  "ApiBaseUrl": "https://localhost:7274"
}
```

| Setting | Description | Required |
|---------|-------------|----------|
| `ApiBaseUrl` | Web API base URL (must match API port) | Yes |

**Important:** Update `ApiBaseUrl` to match your Web API's actual URL and port.

### Console App Settings (`RickAndMorty.Console/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RickAndMorty;User ID=sa;Password=YOUR_STRONG_PWD;TrustServerCertificate=True;Encrypt=False"
  },
  "RickAndMortyApi": {
    "BaseUrl": "https://rickandmortyapi.com/api",
    "CharacterEndpoint": "/character"
  }
}
```

Uses the same settings as the Web API for database and external API access.

### Environment-Specific Configuration

For development, create `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=RickAndMortyDev;User ID=sa;Password=YOUR_STRONG_PWD;TrustServerCertificate=True;Encrypt=False"
  },
  "CharacterMonitor": {
    "Enabled": true,
    "IntervalMinutes": 1,
    "TestMode": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

This allows faster monitoring intervals and test mode for development.

## API Endpoints

### Characters
- `GET /api/characters` - Get all characters (cached)
- `GET /api/characters/filter?name={name}&status={status}&species={species}` - Filter characters
- `GET /api/characters/planet/{planetName}` - Get characters by planet
- `POST /api/characters` - Add new character

### Episodes
- `GET /api/episodes` - Get all episodes (cached)

### SignalR Hub
- `/hubs/character` - WebSocket endpoint for real-time notifications

## SignalR Events

### Server → Client
- `CharacterAdded` - Fired when a character is manually added
- `NewCharactersDetected` - Fired when background service detects new characters
- `CharacterMonitorTest` - Test event (when TestMode = true)

## Notification System

The application features a real-time notification system with:

- **Bell Icon** - Shows unread notification count
- **Notification Menu** - Displays recent character additions
- **Relative Timestamps** - "Just now", "5m ago", "2h ago"
- **Last Checked Time** - Displays when notifications were last viewed
- **Auto-refresh** - Table updates automatically when new characters are added

## Background Services

### CharacterMonitorHostedService

Monitors the database every N minutes (configurable) and broadcasts SignalR notifications when new characters are detected.

**Test Mode:** Sends periodic test messages to verify SignalR connectivity
**Production Mode:** Only sends notifications when actual changes are detected

## Caching Strategy

- **Characters:** Cached for 5 minutes (based on `CharacterMonitor.IntervalMinutes`)
- **Episodes:** Cached for 5 minutes
- **Cache Invalidation:** Automatically cleared when new characters are added
- **Response Headers:** `from-database` and `last-fetched-at` indicate cache status

## Testing

Run all tests:
```bash
dotnet test
```

### Test Coverage
- **Unit Tests:** `FetchCharactersServiceTests` - HTTP client mocking with Moq
- **Integration Tests:** `GetCharactersQueryTests` - EF Core InMemory database

## Project Highlights

### Design Patterns
- Clean Architecture
- CQRS (Command Query Responsibility Segregation)
- Vertical Slice Architecture
- Repository Pattern (via EF Core)
- Dependency Injection

### .NET 8 Features
- Primary Constructors
- Record Types
- Pattern Matching
- Global Using Directives
- Nullable Reference Types
- Minimal APIs (via Carter)

### Best Practices
- Async/await throughout
- Scoped service lifetimes
- Proper disposal with IAsyncDisposable
- Structured logging
- Exception handling
- Response caching
- CORS configuration

## Troubleshooting

### SignalR Not Connecting

1. Check CORS settings in `RickAndMorty.Web/Program.cs`
2. Verify `ApiBaseUrl` in Blazor WASM `appsettings.json` matches Web API URL
3. Check browser console for connection errors

### Table Not Refreshing

1. Check if cache is preventing refresh (look for `from-database: false` header)
2. Verify `CharacterMonitor.Enabled = true` in API settings
3. Run Console app to add new characters to trigger notifications

### Database Migration Issues

```bash
# Drop and recreate database
cd RickAndMorty.Web
dotnet ef database drop
dotnet ef database update
```

## Future Enhancements

- [ ] Authentication/Authorization with JWT
- [ ] Pagination for large datasets
- [ ] Advanced search with Elasticsearch
- [ ] Character image uploads
- [ ] Episode-Character relationship management
- [ ] Export to CSV/Excel
- [ ] GraphQL API
- [ ] Redis distributed cache
- [ ] Docker containerization

## License

This project is for educational purposes, demonstrating modern .NET 8 development practices.

## Credits

- Data from [Rick and Morty API](https://rickandmortyapi.com/)
- UI components by [MudBlazor](https://mudblazor.com/)

---

**Built with .NET 8 | Clean Architecture | SignalR | Blazor WASM**
