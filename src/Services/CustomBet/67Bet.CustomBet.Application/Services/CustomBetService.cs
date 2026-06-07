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

    public CustomBetService(ICustomBetRepository customBetRepository)
    {
        _customBetRepository = customBetRepository;
    }

    public async Task<CustomBetRequest> CreateRequestAsync(Guid userId, string description)
    {
        var request = new CustomBetRequest(userId, description);

        // W wersji I: Symulujemy wycenę przez AI (AI Suggested Odds)
        // W pełnej wersji tutaj byłoby wywołanie modelu ML.NET
        decimal simulatedAiOdds = 2.50m;
        request.SetAiSuggestedOdds(simulatedAiOdds);

        await _customBetRepository.AddAsync(request);
        return request;
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
