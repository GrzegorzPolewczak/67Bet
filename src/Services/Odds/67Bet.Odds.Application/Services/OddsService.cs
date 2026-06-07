using System;
using System.Threading.Tasks;
using _67Bet.Odds.Application.Interfaces;

namespace _67Bet.Odds.Application.Services;

/*
 * Serwis OddsService implementuje logikę wyliczania kursów (Oddsmaker Engine).
 * Wykorzystuje dane historyczne i algorytmy AI do dynamicznej aktualizacji oferty.
 */
public class OddsService : IOddsService
{
    public Task<decimal> CalculateOddsAsync(decimal probability)
    {
        if (probability <= 0) return Task.FromResult(100.0m);
        if (probability >= 1) return Task.FromResult(1.01m);

        // Uproszczony wzór: Kurs = (1 / P) * (1 - Marża)
        decimal margin = 0.05m; // 5% marży bukmacherskiej
        decimal odds = (1.0m / probability) * (1.0m - margin);

        return Task.FromResult(Math.Max(1.01m, Math.Round(odds, 2)));
    }

    public Task UpdateMarketOddsAsync(Guid marketId)
    {
        // Logika wywołania ML.NET i aktualizacji rynków
        // W wersji I: Symulacja aktualizacji
        return Task.CompletedTask;
    }

    public Task<decimal> GetLiveProbabilityAsync(Guid eventId, string contextJson)
    {
        // Symulacja predykcji na żywo
        return Task.FromResult(0.5m);
    }
}
