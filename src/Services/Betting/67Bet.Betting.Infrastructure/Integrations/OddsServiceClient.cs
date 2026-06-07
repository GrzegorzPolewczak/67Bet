using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    public async Task<IReadOnlyCollection<ExternalOddsEventDto>> GetEventsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching external events from Odds Service");

            var response = await _httpClient.GetAsync("ExternalOdds/events");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Odds Service returned non-success status {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    body);
                return Array.Empty<ExternalOddsEventDto>();
            }

            var events = await response.Content.ReadFromJsonAsync<List<ExternalOddsEventDto>>();
            if (events is { Count: > 0 })
                return events;

            _logger.LogInformation("Odds Service returned no events. Triggering external odds sync and retrying once.");
            await _httpClient.PostAsync("ExternalOdds/sync", null);

            var retryResponse = await _httpClient.GetAsync("ExternalOdds/events");
            if (!retryResponse.IsSuccessStatusCode)
                return Array.Empty<ExternalOddsEventDto>();

            var syncedEvents = await retryResponse.Content.ReadFromJsonAsync<List<ExternalOddsEventDto>>();
            return syncedEvents ?? new List<ExternalOddsEventDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch external events from Odds Service");
            return Array.Empty<ExternalOddsEventDto>();
        }
    }

    public async Task<ExternalMatchDto?> GetEventByIdAsync(string eventId)
    {
        try
        {
            _logger.LogInformation("Fetching event {EventId} from Odds Service", eventId);

            var events = await GetEventsAsync();
            var match = events.FirstOrDefault(e => e.Id == eventId);

            if (match == null) return null;

            return new ExternalMatchDto
            {
                Name = BuildEventName(match),
                SportKey = string.IsNullOrWhiteSpace(match.SportTitle) ? match.SportKey : match.SportTitle,
                RecentScores = match.RecentScores,
                CurrentOdds = match.Bookmakers.Count > 0 ? JsonSerializer.Serialize(match.Bookmakers) : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch event {EventId} from Odds Service", eventId);
            return null;
        }
    }

    private static string BuildEventName(ExternalOddsEventDto match)
    {
        if (!string.IsNullOrWhiteSpace(match.HomeTeam) && !string.IsNullOrWhiteSpace(match.AwayTeam))
            return $"{match.HomeTeam} vs {match.AwayTeam}";

        return !string.IsNullOrWhiteSpace(match.HomeTeam)
            ? match.HomeTeam
            : !string.IsNullOrWhiteSpace(match.AwayTeam)
                ? match.AwayTeam
                : "External Event";
    }
}
