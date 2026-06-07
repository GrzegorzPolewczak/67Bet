using _67Bet.Betting.Domain.Entities.Gamification;

namespace _67Bet.Betting.Domain.Repositories;

public interface IUserGamificationRepository
{
    Task<UserGamification?> GetByUserIdAsync(Guid userId);
    Task AddAsync(UserGamification userGamification);
    Task UpdateAsync(UserGamification userGamification);
}
