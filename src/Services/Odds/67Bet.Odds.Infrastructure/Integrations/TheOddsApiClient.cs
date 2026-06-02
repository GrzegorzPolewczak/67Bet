using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Infrastructure.Integrations;

public class TheOddsApiClient : ITheOddsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<TheOddsApiClient> _logger;

    public TheOddsApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TheOddsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["SportsApi:ApiKey"] ?? throw new InvalidOperationException("Sports API Key is missing in configuration.");
    }

    public async Task<IEnumerable<ExternalEventDto>> GetUpcomingEventsAsync(string sport = "upcoming", string regions = "eu,us,uk", string markets = "h2h")
    {
        try
        {
            var url = $"v4/sports/{sport}/odds/?apiKey={_apiKey}&regions={regions}&markets={markets}&oddsFormat=decimal";
            var response = await _httpClient.GetFromJsonAsync<List<ExternalEventDto>>(url);
            return response ?? new List<ExternalEventDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from The Odds API");
            return new List<ExternalEventDto>();
        }
    }

    public async Task<string> GetScoresRawAsync(string sport, int daysFrom = 3)
    {
        try
        {
            var url = $"v4/sports/{sport}/scores/?apiKey={_apiKey}&daysFrom={daysFrom}";
            var response = await _httpClient.GetStringAsync(url);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching scores from The Odds API for sport {Sport}", sport);
            return string.Empty;
        }
    }
}
