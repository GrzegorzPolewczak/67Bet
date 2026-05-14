using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _67Bet.Odds.Api.Hubs;
using _67Bet.Odds.Application.DTOs;
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
    private readonly Random _random = new();
    
    // Dynamiczna lista stanów dla WSZYSTKICH aktywnych meczów w bazie
    private readonly ConcurrentDictionary<string, LiveMatchStateDto> _mockStates = new();

    public LiveTrackerBackgroundService(IHubContext<LiveTrackerHub> hubContext, IServiceScopeFactory scopeFactory, ILogger<LiveTrackerBackgroundService> logger)
    {
        _hubContext = hubContext;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Live Tracker Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IExternalEventRepository>();
                    var activeEvents = await repo.GetAllActiveAsync();

                    foreach (var evt in activeEvents)
                    {
                        if (!_mockStates.ContainsKey(evt.ExternalId))
                        {
                            // Obliczanie faktycznego czasu trwania meczu
                            var timeElapsed = DateTime.UtcNow - evt.StartTime;
                            var startMinutes = timeElapsed.TotalMinutes > 0 ? (int)timeElapsed.TotalMinutes : 0;
                            var startSeconds = timeElapsed.TotalSeconds > 0 ? (int)timeElapsed.Seconds : 0;

                            _mockStates[evt.ExternalId] = new LiveMatchStateDto
                            {
                                MatchId = evt.ExternalId,
                                SportKey = evt.SportKey,
                                CurrentTime = $"{startMinutes:D2}:{startSeconds:D2}",
                                CurrentAction = timeElapsed.TotalMinutes < 0 ? "Upcoming" : "Match Started",
                                Score = new Dictionary<string, string> { { "Home", "0" }, { "Away", "0" } },
                                Statistics = new Dictionary<string, int> { { "Corners", 0 }, { "Fouls", 0 }, { "PossessionHome", 50 } }
                            };
                        }
                    }
                }

                foreach (var state in _mockStates.Values)
                {
                    SimulateMatchUpdate(state);
                    await _hubContext.Clients.Group(state.MatchId).SendAsync("ReceiveMatchUpdate", state, cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Live Tracker update loop.");
            }

            // Aktualizuj co 5 sekund
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private void SimulateMatchUpdate(LiveMatchStateDto match)
    {
        var isSoccer = match.SportKey.Contains("soccer", StringComparison.OrdinalIgnoreCase);
        var isBasketball = match.SportKey.Contains("basketball", StringComparison.OrdinalIgnoreCase);
        var isEsport = match.SportKey.Contains("esport", StringComparison.OrdinalIgnoreCase);

        // Globalna aktualizacja czasu dla wszystkich sportów
        var timeParts = match.CurrentTime.Split(':');
        if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int minutes) && int.TryParse(timeParts[1], out int seconds))
        {
            seconds += 5;
            if (seconds >= 60)
            {
                seconds -= 60;
                minutes++;
            }
            match.CurrentTime = $"{minutes:D2}:{seconds:D2}";
        }

        if (isSoccer)
        {
            var actions = new[] { "Safe Possession", "Dangerous Attack", "Shot on Target", "Goal Kick", "Free Kick" };
            match.CurrentAction = actions[_random.Next(actions.Length)];
            
            if (_random.Next(100) < 2) 
            {
                int scoreHome = int.Parse(match.Score["Home"]);
                match.Score["Home"] = (scoreHome + 1).ToString();
                match.CurrentAction = "GOAL!";
            }
            if (_random.Next(100) < 10) match.Statistics["Corners"]++;
            match.Statistics["PossessionHome"] = _random.Next(40, 61);
        }
        else if (isBasketball)
        {
            var actions = new[] { "Attack", "Defense", "Free Throws", "Timeout" };
            match.CurrentAction = actions[_random.Next(actions.Length)];
            
            if (_random.Next(100) < 30) 
            {
                int scoreHome = int.Parse(match.Score["Home"]);
                match.Score["Home"] = (scoreHome + _random.Next(1, 4)).ToString();
            }
        }
        else if (isEsport)
        {
            var actions = new[] { "Farming", "Team Fight", "Objective Taken", "Pushing Base" };
            match.CurrentAction = actions[_random.Next(actions.Length)];

            if (_random.Next(100) < 15) 
            {
                int killsHome = int.Parse(match.Score["Home"]);
                match.Score["Home"] = (killsHome + 1).ToString();
            }
        }
        else 
        {
            match.CurrentAction = "In Progress";
        }
    }
}
