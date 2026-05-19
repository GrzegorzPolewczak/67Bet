namespace _67Bet.Betting.Application.Interfaces;

public interface IAiAssistantService
{
    Task<string> GetMatchInsightAsync(string eventId);
}

public interface IGeminiClient
{
    Task<string> GenerateTextAsync(string prompt);
}

public interface IOddsServiceClient
{
    Task<ExternalMatchDto?> GetEventByIdAsync(string eventId);
}

public class ExternalMatchDto
{
    public string Name { get; set; } = string.Empty;
    public string SportKey { get; set; } = string.Empty;
}
