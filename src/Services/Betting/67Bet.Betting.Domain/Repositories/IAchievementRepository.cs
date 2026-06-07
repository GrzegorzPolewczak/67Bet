using _67Bet.Betting.Domain.Entities.Gamification;

namespace _67Bet.Betting.Domain.Repositories;

public interface IAchievementRepository
{
    Task<IEnumerable<Achievement>> GetAllAsync();
    Task<Achievement?> GetByIdAsync(Guid id);
    Task AddAsync(Achievement achievement);
}
