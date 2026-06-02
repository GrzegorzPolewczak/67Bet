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
        // 1. Sprawdź cache w bazie danych (tylko dla lokalnych Guid)
        if (Guid.TryParse(eventId, out var eventGuid))
        {
            var existingInsight = await _insightRepository.GetByEventIdAsync(eventGuid);
            // Sprawdzamy też ważność cache (np. odrzucamy starsze niż 24h, choć na razie bazujemy po prostu na istnieniu)
            if (existingInsight != null && existingInsight.GeneratedAt > DateTime.UtcNow.AddHours(-12))
            {
                _logger.LogInformation("Returning cached AI insight for event {EventId}", eventId);
                return existingInsight.Content;
            }
        }

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
                sportMetadata = localMatch.Metadata; // Lub sport ID z repo
                // Dla lokalnych wydarzeń na razie nie mamy wpiętych wyników, AI wygeneruje analizę na bazie samego ułożenia.
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
        try
        {
            var generatedText = await _geminiClient.GenerateTextAsync(prompt);

            // 5. Zapisz lub zaktualizuj w bazie (Caching)
            if (Guid.TryParse(eventId, out var g))
            {
                var existingInsight = await _insightRepository.GetByEventIdAsync(g);
                if (existingInsight == null)
                {
                    var newInsight = new AiMatchInsight(g, generatedText);
                    await _insightRepository.AddOrUpdateAsync(newInsight);
                }
                else
                {
                    existingInsight.UpdateInsight(generatedText);
                    await _insightRepository.AddOrUpdateAsync(existingInsight);
                }
            }

            return generatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI insight for event {EventId}", eventId);
            return "W tej chwili nie możemy wygenerować analizy opartej o fakty. Spróbuj później!";
        }
    }

    private string BuildContextPrompt(string matchName, string sportMetadata, string scores, string odds)
    {
        var teamParts = matchName.Split(" vs ");
        var homeTeam = teamParts.Length > 0 ? teamParts[0].Trim() : matchName;
        var awayTeam = teamParts.Length > 1 ? teamParts[1].Trim() : "";

        return $@"Jesteś wszechstronnym ekspertem i analitykiem bukmacherskim. Poniżej otrzymujesz TWARDE DANE o nadchodzącym wydarzeniu sportowym (Context).
Napisz krótką, trafną analizę (max 2 zdania) na podstawie TYLKO I WYŁĄCZNIE dostarczonych danych. 
Nie zmyślaj żadnych informacji, nie wymyślaj nazwisk, map, punktów ani kontuzji. Bądź konkretny i profesjonalny.

--- KONTEKST ---
Wydarzenie: {homeTeam} vs {awayTeam}
Dyscyplina / Liga: {sportMetadata}

Ostatnie wyniki (JSON - znajdź historię dla powyższych uczestników):
{scores}

Aktualne kursy rynkowe (JSON):
{odds}
--- KONIEC KONTEKSTU ---

ZASADY:
1. ROZPOZNAJ DYSCYPLINĘ z pola 'Dyscyplina / Liga' (np. Piłka nożna, Koszykówka, Tenis, CS:GO, MMA, LoL) i używaj poprawnej dla niej terminologii (np. gole, punkty, mapy, sety, rundy).
2. UŻYWAJ pełnych nazw uczestników/drużyn ({homeTeam} oraz {awayTeam}) wewnątrz tekstu.
3. Skup się na twardych danych: kto jest faworytem według kursów i jaka jest forma z ostatnich wyników.
4. Jeśli dane z JSON są puste lub nie dotyczą tych uczestników, napisz zwięzłe podsumowanie szans WYŁĄCZNIE na podstawie samych kursów, bez dopisywania wymyślonej historii.
5. Od razu przejdź do rzeczy. Nigdy nie zaczynaj odpowiedzi od słów typu ""Oto analiza"", ""Z dostarczonych danych wynika"".";
    }
}
