using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.CustomBet.Domain.Entities;

namespace _67Bet.CustomBet.Application.Interfaces;

/*
 * Interfejs ICustomBetService umożliwia graczom zgłaszanie propozycji własnych zakładów.
 * Definiuje metody dla graczy (składanie wniosków) oraz dla administratorów (akceptacja/odrzucenie).
 */
public interface ICustomBetService
{
    Task<CustomBetRequest> CreateRequestAsync(Guid userId, string description);
    Task<IEnumerable<CustomBetRequest>> GetUserRequestsAsync(Guid userId);
    Task<IEnumerable<CustomBetRequest>> GetPendingRequestsAsync();
    Task AcceptRequestAsync(Guid requestId, decimal finalOdds, string? adminNote = null);
    Task RejectRequestAsync(Guid requestId, string reason);
    Task<CustomBetRequest> GetAiRecommendationAsync(Guid requestId);
}

public interface IGeminiClient
{
    Task<string> GenerateTextAsync(string prompt);
}
