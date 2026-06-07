using _67Bet.Betting.Domain.Entities.Roulette;

namespace _67Bet.Betting.Domain.Repositories;

public interface IRouletteRoundRepository
{
    Task AddAsync(RouletteRound round);
    Task<RouletteRound?> GetByIdAsync(Guid roundId);
    Task UpdateAsync(RouletteRound round);
    Task<IReadOnlyCollection<RouletteRound>> GetRecentForUserAsync(Guid userId, int limit);
}
