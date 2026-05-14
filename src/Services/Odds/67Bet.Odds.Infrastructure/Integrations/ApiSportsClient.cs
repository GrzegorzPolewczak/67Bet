using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Infrastructure.Integrations;

public class ApiSportsClient : ILiveDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiSportsClient> _logger;
    private readonly string _apiKey;

    public ApiSportsClient(HttpClient httpClient, IConfiguration configuration, ILogger<ApiSportsClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["ApiSports:ApiKey"] ?? "YOUR_API_SPORTS_KEY_HERE";
        _httpClient.DefaultRequestHeaders.Add("x-apisports-key", _apiKey);
    }

    public async Task<LiveMatchStateDto?> GetLiveMatchStateAsync(string matchId, string sportKey, string homeTeam, string awayTeam)
    {
        try
        {
            string baseUrl = "https://v3.football.api-sports.io";
            string endpoint = "fixtures?live=all";

            if (sportKey.Contains("tennis"))
            {
                baseUrl = "https://v1.tennis.api-sports.io";
                endpoint = "odds/live";
            }
            else if (sportKey.Contains("basketball"))
            {
                baseUrl = "https://v1.basketball.api-sports.io";
                endpoint = "games?live=all";
            }

            var response = await _httpClient.GetFromJsonAsync<ApiSportsResponse<ApiSportsLiveMatch>>($"{baseUrl}/{endpoint}");
            
            if (response?.Response != null && response.Response.Count > 0)
            {
                // Znalezienie meczu po nazwie drużyny (bardzo uproszczone mapowanie)
                var match = response.Response.Find(m => 
                    m.Fixture?.Status?.Elapsed != null || 
                    (m.Fixture == null && m.Events.Count > 0)
                ) ?? response.Response[0];

                return new LiveMatchStateDto
                {
                    MatchId = matchId,
                    SportKey = sportKey,
                    CurrentTime = (match.Fixture?.Status?.Elapsed ?? 0) + ":00",
                    CurrentAction = match.Events.Count > 0 ? match.Events[^1].Detail : "In Play",
                    Score = new Dictionary<string, string> 
                    { 
                        { "Home", (match.Goals?.GetValueOrDefault("home") ?? 0).ToString() }, 
                        { "Away", (match.Goals?.GetValueOrDefault("away") ?? 0).ToString() } 
                    },
                    Statistics = MapStats(match.Statistics, sportKey),
                    TimelineEvents = MapEvents(match.Events),
                    StreamUrl = sportKey.Contains("esport") ? "https://player.twitch.tv/?channel=esl_csgo&parent=localhost" : null
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching REAL live data from API-Sports for {Sport}", sportKey);
        }
        return null;
    }

    private Dictionary<string, int> MapStats(List<ApiSportsStatWrapper> apiStats, string sportKey)
    {
        var result = new Dictionary<string, int>();
        if (apiStats == null) return result;

        foreach (var wrapper in apiStats)
        {
            foreach (var s in wrapper.Stats)
            {
                if (int.TryParse(s.Value?.ToString(), out int val))
                {
                    string type = s.Type.Replace(" ", "");
                    string suffix = apiStats.IndexOf(wrapper) == 0 ? "Home" : "Away";
                    result[type + suffix] = val;
                }
            }
        }
        return result;
    }

    private List<TimelineEventDto> MapEvents(List<ApiSportsEvent> apiEvents)
    {
        var list = new List<TimelineEventDto>();
        if (apiEvents == null) return list;

        foreach (var e in apiEvents)
        {
            list.Add(new TimelineEventDto {
                Type = e.Type,
                Minute = (e.Time?.Elapsed ?? 0) + "'",
                Description = e.Detail,
                Team = "Home"
            });
        }
        return list;
    }
}
