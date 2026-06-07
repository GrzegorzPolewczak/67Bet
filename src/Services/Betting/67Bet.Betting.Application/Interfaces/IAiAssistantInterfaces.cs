using System.Text.Json.Serialization;

namespace _67Bet.Betting.Application.Interfaces;

public interface IAiAssistantService
{
    Task<string> GetMatchInsightAsync(string eventId);
    Task<IEnumerable<AiMatchInsightDto>> GetAllInsightsAsync();
    Task<string> RegenerateInsightAsync(string eventId);
    Task<bool> DeleteInsightAsync(string eventId);
}

public class AiMatchInsightDto
{
    public string EventId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public interface IGeminiClient
{
    Task<string> GenerateTextAsync(string prompt);
}

public interface IOddsServiceClient
{
    Task<ExternalMatchDto?> GetEventByIdAsync(string eventId);
    Task<IReadOnlyCollection<ExternalOddsEventDto>> GetEventsAsync();
}

public class ExternalMatchDto
{
    public string Name { get; set; } = string.Empty;
    public string SportKey { get; set; } = string.Empty;
    public string? RecentScores { get; set; }
    public string? CurrentOdds { get; set; }
}

public class ExternalOddsEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; } = string.Empty;

    [JsonPropertyName("sport_title")]
    public string SportTitle { get; set; } = string.Empty;

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = string.Empty;

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = string.Empty;

    public string? StreamUrl { get; set; }
    public string? RecentScores { get; set; }

    [JsonPropertyName("bookmakers")]
    public List<ExternalOddsBookmakerDto> Bookmakers { get; set; } = new();
}

public class ExternalOddsBookmakerDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("markets")]
    public List<ExternalOddsMarketDto> Markets { get; set; } = new();
}

public class ExternalOddsMarketDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("outcomes")]
    public List<ExternalOddsOutcomeDto> Outcomes { get; set; } = new();
}

public class ExternalOddsOutcomeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}
