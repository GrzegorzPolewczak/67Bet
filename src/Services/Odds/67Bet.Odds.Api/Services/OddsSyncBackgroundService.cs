using System;
using System.Threading;
using System.Threading.Tasks;
using _67Bet.Odds.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Api.Services;

public class OddsSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OddsSyncBackgroundService> _logger;

    public OddsSyncBackgroundService(IServiceProvider serviceProvider, ILogger<OddsSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Odds Sync Background Service started. Will sync every 4 hours.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var integrationService = scope.ServiceProvider.GetRequiredService<IOddsIntegrationService>();
                    _logger.LogInformation("Starting automated external odds sync...");
                    
                    var result = await integrationService.SyncExternalOddsAsync();
                    
                    _logger.LogInformation("Finished automated odds sync. Processed: {Processed}, Added: {Added}, Errors: {ErrorsCount}", 
                        result.EventsProcessed, result.NewEventsAdded, result.Errors.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while syncing external odds.");
            }

            // Oczekuj 4 godziny przed kolejną synchronizacją
            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}
