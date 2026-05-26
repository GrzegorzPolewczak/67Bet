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
        // 1. Sprawdź cache w bazie danych (używając string jako ID dla AiMatchInsight jeśli tak jest w bazie, 
        // ale baza używa Guid dla EventId. Jeśli to zewnętrzny event, musimy obsłużyć cache inaczej lub 
        // zmienić AiMatchInsight, aby akceptował string jako EventId.
        // Zakładamy na razie, że AiMatchInsight.EventId jest Guidem, więc cache działa tylko dla lokalnych.

        if (Guid.TryParse(eventId, out var eventGuid))
        {
            var existingInsight = await _insightRepository.GetByEventIdAsync(eventGuid);
            if (existingInsight != null)
            {
                _logger.LogInformation("Returning cached AI insight for event {EventId}", eventId);
                return existingInsight.Content;
            }
        }

        // 2. Pobierz dane o meczu do promptu
        string matchName;
        string sportMetadata;

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
                // Próba pobrania z zewnętrznego serwisu kursów
                var externalMatch = await _oddsServiceClient.GetEventByIdAsync(eventId);
                if (externalMatch == null)
                {
                    throw new KeyNotFoundException($"Event with ID {eventId} not found locally or in Odds Service.");
                }
                matchName = externalMatch.Name;
                sportMetadata = externalMatch.SportKey;
            }
        }
        else
        {
            // ID nie jest Guidem, więc musi to być zewnętrzny event (string ID z The Odds API)
            var externalMatch = await _oddsServiceClient.GetEventByIdAsync(eventId);
            if (externalMatch == null)
            {
                throw new KeyNotFoundException($"External event with ID {eventId} not found in Odds Service.");
            }
            matchName = externalMatch.Name;
            sportMetadata = externalMatch.SportKey;
        }

        // 3. Przygotuj prompt dla Gemini
        var teamParts = matchName.Split(" vs ");
        var homeTeam = teamParts.Length > 0 ? teamParts[0] : matchName;
        var awayTeam = teamParts.Length > 1 ? teamParts[1] : "";

        var prompt = $@"Jesteś profesjonalnym analitykiem sportowym. Na podstawie danych o meczu napisz JEDNĄ konkretną podpowiedź (max 2 zdania).

DANE MECZU:
Gospodarz: {homeTeam}
Gość: {awayTeam}
Liga/Dyscyplina: {sportMetadata}

PRZYKŁAD POPRAWNEJ ODPOWIEDZI (Dla meczu Real Madryt vs Barcelona):
""Real Madryt wygrał 4 z ostatnich 5 spotkań u siebie z Barceloną. Biorąc pod uwagę powrót ich kluczowego napastnika, gospodarze są faworytem do objęcia prowadzenia już w pierwszej połowie.""

ZASADY DLA TWOJEJ ODPOWIEDZI:
1. UŻYWAJ PEŁNYCH NAZW DRUŻYN ({homeTeam} oraz {awayTeam}) wewnątrz tekstu.
2. NIGDY nie zostawiaj pustych miejsc ani kropek zamiast nazw.
3. Skup się na jednym fakcie: formie, H2H lub kluczowym graczu.
4. Nie używaj wstępów typu ""Oto analiza"". Napisz samą treść podpowiedzi.";

        // 4. Pobierz analizę z Gemini API
        try
        {
            var generatedText = await _geminiClient.GenerateTextAsync(prompt);

            // 5. Zapisz w bazie (Caching) - tylko jeśli mamy Guid (lokalny event)
            if (Guid.TryParse(eventId, out var g))
            {
                var newInsight = new AiMatchInsight(g, generatedText);
                await _insightRepository.AddAsync(newInsight);
            }

            return generatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI insight for event {EventId}", eventId);
            return "W tej chwili nie możemy wygenerować analizy. Spróbuj później!";
        }
    }
}
