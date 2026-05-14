using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Infrastructure.Integrations;

// Modele specyficzne dla API PandaScore
public class PandaScoreMatch
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("begin_at")]
    public DateTime? BeginAt { get; set; }
    
    [JsonPropertyName("videogame")]
    public PandaScoreVideoGame VideoGame { get; set; } = new();
    
    [JsonPropertyName("opponents")]
    public List<PandaScoreOpponentWrapper> Opponents { get; set; } = new();
}

public class PandaScoreVideoGame
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty; // np. "cs-go", "league-of-legends"
}

public class PandaScoreOpponentWrapper
{
    [JsonPropertyName("opponent")]
    public PandaScoreOpponent Opponent { get; set; } = new();
}

public class PandaScoreOpponent
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class PandaScoreApiClient : IPandaScoreApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PandaScoreApiClient> _logger;
    private readonly string _apiKey;

    public PandaScoreApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<PandaScoreApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["PandaScoreApi:ApiKey"] ?? throw new InvalidOperationException("PandaScore API Key missing");
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IEnumerable<ExternalEventDto>> GetUpcomingEsportsMatchesAsync()
    {
        try
        {
            // Pobieranie nadchodzących meczów z PandaScore
            var url = "matches/upcoming?sort=begin_at&per_page=15";
            var response = await _httpClient.GetFromJsonAsync<List<PandaScoreMatch>>(url);
            
            var mappedEvents = new List<ExternalEventDto>();
            
            if (response != null)
            {
                foreach (var match in response)
                {
                    if (match.Opponents.Count >= 2 && match.BeginAt.HasValue)
                    {
                        var homeTeam = match.Opponents[0].Opponent.Name;
                        var awayTeam = match.Opponents[1].Opponent.Name;
                        var sportKey = "esports_" + match.VideoGame.Slug.Replace("-", "");

                        var eventDto = new ExternalEventDto
                        {
                            Id = "ps_" + match.Id.ToString(), // Unikalny prefix, żeby nie było konfliktu z TheOddsApi
                            SportKey = sportKey,
                            SportTitle = "Esports " + match.VideoGame.Slug.ToUpper(),
                            CommenceTime = match.BeginAt.Value,
                            HomeTeam = homeTeam,
                            AwayTeam = awayTeam,
                            Bookmakers = new List<BookmakerDto>
                            {
                                new BookmakerDto
                                {
                                    Key = "pandascore_mock",
                                    Title = "PandaScore Default",
                                    Markets = new List<ExternalMarketDto>
                                    {
                                        new ExternalMarketDto
                                        {
                                            Key = "h2h",
                                            Outcomes = new List<ExternalOutcomeDto>
                                            {
                                                new ExternalOutcomeDto { Name = homeTeam, Price = 1.85m },
                                                new ExternalOutcomeDto { Name = awayTeam, Price = 1.95m }
                                            }
                                        }
                                    }
                                }
                            }
                        };
                        mappedEvents.Add(eventDto);
                    }
                }
            }
            return mappedEvents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from PandaScore API");
            return new List<ExternalEventDto>();
        }
    }
}
