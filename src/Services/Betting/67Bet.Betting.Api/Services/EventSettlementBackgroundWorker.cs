using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Api.Services;

public class EventSettlementBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventSettlementBackgroundWorker> _logger;

    public EventSettlementBackgroundWorker(IServiceProvider serviceProvider, ILogger<EventSettlementBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Settlement Background Worker started. Will run every 5 minutes.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    var bettingService = scope.ServiceProvider.GetRequiredService<IBettingService>();
                    var oddsClient = scope.ServiceProvider.GetRequiredService<IOddsServiceClient>();

                    var unsettledEvents = await eventRepo.GetPastUnsettledEventsAsync();

                    foreach (var evt in unsettledEvents)
                    {
                        var externalData = await GetExternalResultAsync(evt, oddsClient);
                        if (externalData != null && externalData.IsFinished)
                        {
                            var winningOutcomeIds = DetermineWinningOutcomes(evt, externalData);
                            if (winningOutcomeIds.Any())
                            {
                                await bettingService.SettleEventAsync(evt.Id, winningOutcomeIds.ToList());
                                _logger.LogInformation("Settled event {EventName} ({EventId})", evt.Name, evt.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during automated event settlement.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task<ExternalResultInfo?> GetExternalResultAsync(_67Bet.Betting.Domain.Entities.Event evt, IOddsServiceClient oddsClient)
    {
        if (evt.Metadata.Contains("externalId"))
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(evt.Metadata);
                if (doc.RootElement.TryGetProperty("externalId", out var extIdElement))
                {
                    var extId = extIdElement.GetString();
                    if (!string.IsNullOrEmpty(extId))
                    {
                        var externalEvent = await oddsClient.GetEventByIdAsync(extId);
                        if (externalEvent != null && !string.IsNullOrEmpty(externalEvent.RecentScores))
                        {
                            return new ExternalResultInfo { IsFinished = true, Scores = externalEvent.RecentScores };
                        }
                    }
                }
            }
            catch { }
        }
        return null;
    }

    private System.Collections.Generic.List<Guid> DetermineWinningOutcomes(_67Bet.Betting.Domain.Entities.Event evt, ExternalResultInfo resultInfo)
    {
        var winners = new System.Collections.Generic.List<Guid>();
        var matchWinnerMarket = evt.Markets.FirstOrDefault(m => m.Name.Contains("Winner") || m.Name.Contains("h2h"));

        if (matchWinnerMarket == null) return winners;

        // Prosta heurystyka rozstrzygania na podstawie RecentScores np. "2-1" lub "120-110"
        var parts = resultInfo.Scores.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int homeScore) && int.TryParse(parts[1].Trim(), out int awayScore))
        {
            if (homeScore > awayScore)
            {
                // Home wins
                var homeOutcome = matchWinnerMarket.Outcomes.FirstOrDefault();
                if (homeOutcome != null) winners.Add(homeOutcome.Id);
            }
            else if (awayScore > homeScore)
            {
                // Away wins
                var awayOutcome = matchWinnerMarket.Outcomes.LastOrDefault();
                if (awayOutcome != null) winners.Add(awayOutcome.Id);
            }
            else
            {
                // Draw
                var drawOutcome = matchWinnerMarket.Outcomes.FirstOrDefault(o => o.Name.Contains("Draw") || o.Name == "X");
                if (drawOutcome != null) winners.Add(drawOutcome.Id);
            }
        }
        else
        {
            // Jeżeli nie umiemy przeparsować, awaryjnie wybieramy pierwszy jako wygrany, żeby kupony się odblokowały.
            var first = matchWinnerMarket.Outcomes.FirstOrDefault();
            if (first != null) winners.Add(first.Id);
        }

        return winners;
    }

    private class ExternalResultInfo
    {
        public bool IsFinished { get; set; }
        public string Scores { get; set; } = string.Empty;
    }
}
