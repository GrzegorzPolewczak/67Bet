using System;
using System.Threading.Tasks;

namespace _67Bet.Odds.Application.Interfaces;

/*
 * Interfejs IOddsService zarządza procesem generowania i aktualizacji kursów.
 * Integruje silnik ML.NET do przewidywania prawdopodobieństw i wyliczania kursów bukmacherskich.
 */
public interface IOddsService
{
    Task<decimal> CalculateOddsAsync(decimal probability);
    Task UpdateMarketOddsAsync(Guid marketId);
    Task<decimal> GetLiveProbabilityAsync(Guid eventId, string contextJson);
}
