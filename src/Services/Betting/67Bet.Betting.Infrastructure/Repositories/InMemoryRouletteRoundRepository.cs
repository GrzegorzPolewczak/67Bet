using System.Collections.Concurrent;
using _67Bet.Betting.Domain.Entities.Roulette;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Infrastructure.Repositories;

public sealed class InMemoryRouletteRoundRepository : IRouletteRoundRepository
{
    private static readonly ConcurrentDictionary<Guid, RouletteRound> Rounds = new();

    public Task AddAsync(RouletteRound round)
    {
        Rounds[round.Id] = round;
        return Task.CompletedTask;
    }

    public Task<RouletteRound?> GetByIdAsync(Guid roundId)
    {
        Rounds.TryGetValue(roundId, out var round);
        return Task.FromResult(round);
    }

    public Task UpdateAsync(RouletteRound round)
    {
        Rounds[round.Id] = round;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<RouletteRound>> GetRecentForUserAsync(Guid userId, int limit)
    {
        var rounds = Rounds.Values
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<RouletteRound>>(rounds);
    }
}
