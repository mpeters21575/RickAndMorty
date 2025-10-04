using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;
using RickAndMorty.Web.CrossCutting;
using RickAndMorty.Web.Hubs;

namespace RickAndMorty.Web.Services;

public sealed class CharacterMonitorHostedService(
    IServiceProvider serviceProvider,
    IHubContext<CharacterHub> hubContext,
    IOptions<CharacterMonitorSettings> settings,
    ILogger<CharacterMonitorHostedService> logger)
    : BackgroundService
{
    private readonly CharacterMonitorSettings _settings = settings.Value;
    private int _lastProcessedId = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Character monitoring disabled");
            return;
        }

        logger.LogInformation("Character monitor started. Interval: {Minutes}min", _settings.IntervalMinutes);

        await InitializeLastProcessedIdAsync(stoppingToken);

        var intervalTimeSpan = TimeSpan.FromMinutes(_settings.IntervalMinutes);
        logger.LogInformation("Waiting {Seconds} seconds before first check...", intervalTimeSpan.TotalSeconds);

        using var timer = new PeriodicTimer(intervalTimeSpan);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Waiting for next timer tick...");
                var timerFired = await timer.WaitForNextTickAsync(stoppingToken);
                
                if (timerFired)
                {
                    logger.LogInformation("Timer fired! Checking for new characters...");
                    await CheckForNewCharactersAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Monitoring cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in monitoring loop");
            }
        }

        logger.LogInformation("Character monitoring stopped");
    }

    private async Task InitializeLastProcessedIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            _lastProcessedId = await dbContext.Characters
                .MaxAsync(c => (int?)c.Id, cancellationToken) ?? 0;
            
            logger.LogInformation("Initialized at character ID: {Id}", _lastProcessedId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error initializing last processed ID");
        }
    }

    private async Task CheckForNewCharactersAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("CheckForNewCharacters: Starting check. Last processed ID: {Id}", _lastProcessedId);
        
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            if (_settings.TestMode)
            {
                var count = await dbContext.Characters.CountAsync(cancellationToken);
                logger.LogInformation("Test mode: {Count} characters", count);
                await hubContext.Clients.All.SendCoreAsync("CharacterMonitorTest", 
                    [$"Monitor running. Total: {count}"], cancellationToken);
                return;
            }

            var newCharacters = await dbContext.Characters
                .Where(c => c.Id > _lastProcessedId)
                .OrderBy(c => c.Id)
                .Select(c => new { c.Id, c.Name, c.ImageUrl })
                .ToListAsync(cancellationToken);

            logger.LogInformation("CheckForNewCharacters: Found {Count} new character(s)", newCharacters.Count);

            if (newCharacters.Count == 0)
            {
                return;
            }

            cache.RemoveByPrefix(CacheKeys.Characters);

            foreach (var character in newCharacters)
            {
                await hubContext.Clients.All.SendCoreAsync("CharacterAdded", 
                    [character.Name, character.ImageUrl ?? string.Empty], cancellationToken);
                
                logger.LogInformation("Broadcasted: {Name} (ID: {Id})", character.Name, character.Id);
            }

            _lastProcessedId = newCharacters.Max(c => c.Id);
            logger.LogInformation("Updated last processed ID to: {Id}", _lastProcessedId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking for new characters");
        }
    }
}