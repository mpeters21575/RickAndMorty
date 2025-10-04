using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RickAndMorty.Infrastructure;
using RickAndMorty.Infrastructure.Configuration;
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
    private int _lastKnownCount;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            logger.LogInformation("Character monitoring is disabled");
            return;
        }

        logger.LogInformation("Character monitoring started. Interval: {Minutes} minutes, Test Mode: {TestMode}", 
            _settings.IntervalMinutes, _settings.TestMode);

        await InitializeLastKnownCountAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckForNewCharactersAsync(stoppingToken);
        }
    }

    private async Task InitializeLastKnownCountAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        _lastKnownCount = await dbContext.Characters.CountAsync(cancellationToken);
        logger.LogInformation("Initial character count: {Count}", _lastKnownCount);
    }

    private async Task CheckForNewCharactersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var currentCount = await dbContext.Characters.CountAsync(cancellationToken);
            var newCharactersCount = currentCount - _lastKnownCount;

            if (_settings.TestMode)
            {
                logger.LogInformation("Test mode: Broadcasting test message");
                await hubContext.Clients.All.SendCoreAsync("CharacterMonitorTest", 
                    [$"Character monitor is running. Current count: {currentCount}"], 
                    cancellationToken);
            }
            else if (newCharactersCount > 0)
            {
                logger.LogInformation("Found {Count} new character(s). Broadcasting update.", newCharactersCount);
                
                var newCharacters = await dbContext.Characters
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(newCharactersCount)
                    .Select(c => c.Name)
                    .ToListAsync(cancellationToken);

                foreach (var characterName in newCharacters)
                {
                    await hubContext.Clients.All.SendCoreAsync("CharacterAdded",
                        [characterName],
                        cancellationToken);
                }

                // Also send the total count notification
                await hubContext.Clients.All.SendCoreAsync("NewCharactersDetected", 
                    [newCharactersCount], 
                    cancellationToken);
                
                _lastKnownCount = currentCount;
            }
            else
            {
                logger.LogDebug("No new characters detected. Count remains: {Count}", currentCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking for new characters");
        }
    }
}