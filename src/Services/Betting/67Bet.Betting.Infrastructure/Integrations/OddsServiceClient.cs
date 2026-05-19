using System.Net.Http.Json;
using _67Bet.Betting.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Infrastructure.Integrations;

public class OddsServiceClient : IOddsServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OddsServiceClient> _logger;

    public OddsServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<OddsServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var baseUrl = configuration["OddsService:BaseUrl"] ?? "http://localhost:5300/api/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<ExternalMatchDto?> GetEventByIdAsync(string eventId)
    {
        try
        {
            // Zakładamy, że Odds API ma punkt końcowy do pobierania pojedynczego wydarzenia
            // W ExternalOddsController widzimy tylko /events (lista wszystkich)
            // Możemy albo pobrać wszystkie i filtrować, albo dodać endpoint w Odds API
            
            _logger.LogInformation("Fetching event {EventId} from Odds Service", eventId);
            
            // Tymczasowo pobieramy wszystkie i szukamy, bo nie widzimy endpointu /events/{id}
            var response = await _httpClient.GetAsync("ExternalOdds/events");
            if (!response.IsSuccessStatusCode) return null;

            var events = await response.Content.ReadFromJsonAsync<List<ExternalEventResponse>>();
            var match = events?.FirstOrDefault(e => e.Id == eventId);

            if (match == null) return null;

            return new ExternalMatchDto
            {
                Name = $"{match.HomeTeam} vs {match.AwayTeam}",
                SportKey = match.SportTitle ?? match.SportKey ?? "Sport"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch event {EventId} from Odds Service", eventId);
            return null;
        }
    }

    private class ExternalEventResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? HomeTeam { get; set; }
        public string? AwayTeam { get; set; }
        public string? SportKey { get; set; }
        public string? SportTitle { get; set; }
    }
}
