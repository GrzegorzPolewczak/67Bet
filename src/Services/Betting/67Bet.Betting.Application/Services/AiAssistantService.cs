using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Application.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly IAiMatchInsightRepository _insightRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IOddsServiceClient _oddsServiceClient;
    private readonly IGeminiClient _geminiClient;
    private readonly ILogger<AiAssistantService> _logger;

    public AiAssistantService(
        IAiMatchInsightRepository insightRepository,
        IEventRepository eventRepository,
        IOddsServiceClient oddsServiceClient,
        IGeminiClient geminiClient,
        ILogger<AiAssistantService> logger)
    {
        _insightRepository = insightRepository;
        _eventRepository = eventRepository;
        _oddsServiceClient = oddsServiceClient;
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<string> GetMatchInsightAsync(string eventId)
    {
        // 1. Sprawdź cache w bazie danych (dla wszystkich identyfikatorów string)
        var existingInsight = await _insightRepository.GetByEventIdAsync(eventId);
        // Sprawdzamy też ważność cache (np. odrzucamy starsze niż 12h)
        if (existingInsight != null && existingInsight.GeneratedAt > DateTime.UtcNow.AddHours(-12))
        {
            _logger.LogInformation("Returning cached AI insight for event {EventId}", eventId);
            return existingInsight.Content;
        }

        return await InternalGenerateAndSaveInsightAsync(eventId);
    }

    public async Task<IEnumerable<AiMatchInsightDto>> GetAllInsightsAsync()
    {
        var insights = await _insightRepository.GetAllAsync();
        return insights.Select(i => new AiMatchInsightDto
        {
            EventId = i.EventId,
            Content = i.Content,
            GeneratedAt = i.GeneratedAt
        });
    }

    public async Task<string> RegenerateInsightAsync(string eventId)
    {
        _logger.LogInformation("Admin requested manual regeneration of AI insight for event {EventId}", eventId);
        return await InternalGenerateAndSaveInsightAsync(eventId);
    }

    public async Task<bool> DeleteInsightAsync(string eventId)
    {
        try
        {
            await _insightRepository.DeleteAsync(eventId);
            _logger.LogInformation("Admin deleted AI insight for event {EventId}", eventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting AI insight for event {EventId}", eventId);
            return false;
        }
    }

    private async Task<string> InternalGenerateAndSaveInsightAsync(string eventId)
    {
        try
        {
            // 2. Pobierz podstawowe dane o meczu oraz zsynchronizowane twarde dane
            string matchName;
            string sportMetadata;
            string recentScores = string.Empty;
            string currentOdds = string.Empty;

            if (Guid.TryParse(eventId, out var guid))
            {
                var localMatch = await _eventRepository.GetByIdAsync(guid);
                if (localMatch != null)
                {
                    matchName = localMatch.Name;
                    sportMetadata = localMatch.Metadata;
                }
                else
                {
                    var externalMatch = await _oddsServiceClient.GetEventByIdAsync(eventId);
                    if (externalMatch == null)
                        throw new KeyNotFoundException($"Event with ID {eventId} not found locally or in Odds Service.");

                    matchName = externalMatch.Name;
                    sportMetadata = externalMatch.SportKey;
                    recentScores = externalMatch.RecentScores ?? string.Empty;
                    currentOdds = externalMatch.CurrentOdds ?? string.Empty;
                }
            }
            else
            {
                var externalMatch = await _oddsServiceClient.GetEventByIdAsync(eventId);
                if (externalMatch == null)
                    throw new KeyNotFoundException($"External event with ID {eventId} not found in Odds Service.");

                matchName = externalMatch.Name;
                sportMetadata = externalMatch.SportKey;
                recentScores = externalMatch.RecentScores ?? string.Empty;
                currentOdds = externalMatch.CurrentOdds ?? string.Empty;
            }

            // 3. Przygotuj prompt dla Gemini z użyciem lokalnie zsynchronizowanych danych
            var prompt = BuildContextPrompt(matchName, sportMetadata, recentScores, currentOdds);

            // 4. Pobierz analizę z Gemini API
            var generatedText = await _geminiClient.GenerateTextAsync(prompt);

            // 5. Zapisz lub zaktualizuj w bazie (Caching)
            var newInsight = new AiMatchInsight(eventId, generatedText);
            await _insightRepository.AddOrUpdateAsync(newInsight);

            // 6. Logowanie sukcesu
            await _insightRepository.AddLogAsync(new AiGenerationLog(eventId, "Success"));

            return generatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during internal AI generation for event {EventId}", eventId);
            await _insightRepository.AddLogAsync(new AiGenerationLog(eventId, "Error", ex.Message));
            return "W tej chwili nie możemy wygenerować analizy opartej o fakty. Spróbuj później!";
        }
    }

    private string BuildContextPrompt(string matchName, string sportMetadata, string scores, string odds)
    {
        var teamParts = matchName.Split(" vs ");
        var homeTeam = teamParts.Length > 0 ? teamParts[0].Trim() : matchName;
        var awayTeam = teamParts.Length > 1 ? teamParts[1].Trim() : "";

        return $@"Jesteś bezkompromisowym ekspertem bukmacherskim i analitykiem sportowym o ogromnej wiedzy. 
Twoim zadaniem jest napisać błyskotliwą, konkretną analizę (max 2 zdania) dla gracza, która pomoże mu podjąć decyzję.

--- DANE DO ANALIZY ---
Wydarzenie: {homeTeam} vs {awayTeam}
Dyscyplina / Liga: {sportMetadata}
Historia ostatnich wyników: {scores}
Aktualne kursy: {odds}
--- KONIEC DANYCH ---

ZASADY ANALIZY:
1. NIGDY nie pisz, że ""brak danych"", ""analiza jest ograniczona"" lub ""nie możesz wygenerować"". Jeśli JSON jest pusty, użyj swojej wiedzy o uczestnikach lub wyciągnij wnioski z kursów (np. duża różnica kursów to dominacja uczestnika A).
2. STYLISTYKA: Pisz pewnie i profesjonalnie. Używaj terminologii sportowej (np. ""twierdza własnego boiska"", ""underdog"", ""clean sheet"", ""map pool"", ""entry fragi"").
3. SKUP SIĘ na uczestnikach: {homeTeam} oraz {awayTeam}.
4. PRZYKŁAD STYLU: ""{homeTeam} to obecnie potęga w tej lidze, co potwierdzają miażdżące kursy. Biorąc pod uwagę słabą formę {awayTeam}, spodziewamy się jednostronnego widowiska i szybkiego rozstrzygnięcia.""
5. Od razu przejdź do rzeczy. Zero wstępów. Ma brzmieć jak analiza od profesjonalnego typerów.";
    }
}
