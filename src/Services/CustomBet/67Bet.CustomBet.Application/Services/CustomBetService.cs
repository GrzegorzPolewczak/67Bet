using System.Text.Json;
using _67Bet.CustomBet.Application.Interfaces;
using _67Bet.CustomBet.Domain.Entities;
using _67Bet.CustomBet.Domain.Repositories;

namespace _67Bet.CustomBet.Application.Services;

/*
 * Serwis CustomBetService zarządza cyklem życia niestandardowych propozycji zakładów.
 * Obsługuje przyjmowanie wniosków, ich procesowanie przez AI oraz finalną decyzję administratora.
 */
public class CustomBetService : ICustomBetService
{
    private readonly ICustomBetRepository _customBetRepository;
    private readonly IGeminiClient _geminiClient;

    public CustomBetService(ICustomBetRepository customBetRepository, IGeminiClient geminiClient)
    {
        _customBetRepository = customBetRepository;
        _geminiClient = geminiClient;
    }

    public async Task<CustomBetRequest> CreateRequestAsync(Guid userId, string description)
    {
        var request = new CustomBetRequest(userId, description);
        await _customBetRepository.AddAsync(request);
        return request;
    }

    public async Task<CustomBetRequest> GetAiRecommendationAsync(Guid requestId)
    {
        var request = await _customBetRepository.GetByIdAsync(requestId);
        if (request == null) throw new InvalidOperationException("Wniosek nie istnieje.");

        var prompt = $@"Jesteś ekspertem bukmacherskim. Przeanalizuj poniższą propozycję zakładu od gracza i zasugeruj kurs (odds), ocenę ryzyka oraz krótkie uzasadnienie.
Zwróć wynik WYŁĄCZNIE w formacie JSON:
{{
  ""odds"": decimal,
  ""risk"": ""Low"" | ""Medium"" | ""High"",
  ""reasoning"": ""string"",
  ""category"": ""string""
}}

PROPOZYCJA GRACZA:
""{request.Description}""";

        try
        {
            var response = await _geminiClient.GenerateTextAsync(prompt);
            // Wyciągnięcie JSONa z odpowiedzi (na wypadek gdyby model dodał ```json ... ```)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            if (jsonStart != -1 && jsonEnd != -1)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var recommendation = JsonSerializer.Deserialize<AiRecommendationResponse>(jsonStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (recommendation != null)
                {
                    request.SetAiRecommendation(
                        recommendation.Odds,
                        recommendation.Reasoning,
                        recommendation.Risk,
                        recommendation.Category);

                    await _customBetRepository.UpdateAsync(request);
                }
            }
        }
        catch (Exception)
        {
            // Fallback w razie błędu AI
            request.SetAiRecommendation(2.0m, "Nie udało się wygenerować analizy AI. Podano kurs domyślny.", "Medium", "General");
            await _customBetRepository.UpdateAsync(request);
        }

        return request;
    }

    private class AiRecommendationResponse
    {
        public decimal Odds { get; set; }
        public string Risk { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public async Task<IEnumerable<CustomBetRequest>> GetUserRequestsAsync(Guid userId)
    {
        return await _customBetRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<CustomBetRequest>> GetPendingRequestsAsync()
    {
        return await _customBetRepository.GetPendingRequestsAsync();
    }

    public async Task AcceptRequestAsync(Guid requestId, decimal finalOdds, string? adminNote = null)
    {
        var request = await _customBetRepository.GetByIdAsync(requestId);
        if (request == null) throw new InvalidOperationException("Wniosek nie istnieje.");

        request.Accept(finalOdds, adminNote);
        await _customBetRepository.UpdateAsync(request);
    }

    public async Task RejectRequestAsync(Guid requestId, string reason)
    {
        var request = await _customBetRepository.GetByIdAsync(requestId);
        if (request == null) throw new InvalidOperationException("Wniosek nie istnieje.");

        request.Reject(reason);
        await _customBetRepository.UpdateAsync(request);
    }
}
