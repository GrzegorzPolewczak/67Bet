using System.Collections.Concurrent;
using _67Bet.Betting.Domain.Entities.Plinko;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Infrastructure.Repositories;

public sealed class InMemoryPlinkoRoundRepository : IPlinkoRoundRepository
{
    private static readonly ConcurrentDictionary<Guid, PlinkoRound> Rounds = new();

    public Task AddAsync(PlinkoRound round)
    {
        Rounds[round.Id] = round;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<PlinkoRound>> GetRecentForUserAsync(Guid userId, int limit)
    {
        var rounds = Rounds.Values
            .Where(round => round.UserId == userId)
            .OrderByDescending(round => round.CreatedAtUtc)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<PlinkoRound>>(rounds);
    }
}
