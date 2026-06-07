using _67Bet.Betting.Domain.Entities.Plinko;

namespace _67Bet.Betting.Domain.Repositories;

public interface IPlinkoRoundRepository
{
    Task AddAsync(PlinkoRound round);
    Task<PlinkoRound?> GetByIdAsync(Guid roundId);
    Task UpdateAsync(PlinkoRound round);
    Task<IReadOnlyCollection<PlinkoRound>> GetRecentForUserAsync(Guid userId, int limit);
}
