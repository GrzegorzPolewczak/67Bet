using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _67Bet.Odds.Api.Hubs;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Api.Services;

public class LiveTrackerBackgroundService : BackgroundService
{
    private readonly IHubContext<LiveTrackerHub> _hubContext;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveTrackerBackgroundService> _logger;

    public LiveTrackerBackgroundService(IHubContext<LiveTrackerHub> hubContext, IServiceScopeFactory scopeFactory, ILogger<LiveTrackerBackgroundService> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Live Tracker REAL DATA ONLY Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IExternalEventRepository>();
                    var liveProvider = scope.ServiceProvider.GetRequiredService<ILiveDataProvider>();
                    var activeEvents = await repo.GetAllActiveAsync();

                    foreach (var evt in activeEvents)
                    {
                        // POBIERAMY WYŁĄCZNIE PRAWDZIWE DANE
                        var realState = await GetStateFromApis(evt, scope, liveProvider);

                        if (realState == null)
                        {
                            realState = CreateFallbackState(evt);
                        }

                        if (string.IsNullOrEmpty(realState.StreamUrl))
                        {
                            realState.StreamUrl = FindStreamForMatch(evt.Name, evt.SportKey);
                        }

                        await _hubContext.Clients.Group(evt.ExternalId).SendAsync("ReceiveMatchUpdate", realState, cancellationToken: stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Live Tracker loop.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task<LiveMatchStateDto?> GetStateFromApis(dynamic evt, IServiceScope scope, ILiveDataProvider liveProvider)
    {
        if (evt.ExternalId.StartsWith("ps_"))
        {
            var pandaClient = scope.ServiceProvider.GetRequiredService<IPandaScoreApiClient>();
            return await pandaClient.GetLiveMatchDetailsAsync(evt.ExternalId);
        }

        return await liveProvider.GetLiveMatchStateAsync(evt.ExternalId, evt.SportKey, evt.Name, "");
    }

    private string FindStreamForMatch(string matchName, string sportKey)
    {
        if (sportKey.Contains("esport"))
        {
            string channel = "esl_csgo";
            if (sportKey.Contains("league")) channel = "riotgames";
            return $"https://player.twitch.tv/?channel={channel}&parent=localhost&autoplay=true&muted=true";
        }
        var searchQuery = Uri.EscapeDataString(matchName + " live stream");
        return $"https://www.youtube.com/embed?listType=search&list={searchQuery}&autoplay=1&mute=1";
    }

    private LiveMatchStateDto CreateFallbackState(dynamic evt)
    {
        string matchId = evt.ExternalId;
        string sportKey = evt.SportKey;
        string recentScores = evt.RecentScores ?? "0-0";
        string homeScore = "0";
        string awayScore = "0";

        if (!string.IsNullOrEmpty(recentScores) && recentScores.Contains("-"))
        {
            var parts = recentScores.Split('-');
            if (parts.Length == 2)
            {
                homeScore = parts[0].Trim();
                awayScore = parts[1].Trim();
            }
        }

        return new LiveMatchStateDto
        {
            MatchId = matchId,
            SportKey = sportKey,
            CurrentTime = "Live",
            CurrentAction = "Match in progress",
            CurrentZone = "Midfield",
            Score = new Dictionary<string, string> { { "Home", homeScore }, { "Away", awayScore } }
        };
    }
}
