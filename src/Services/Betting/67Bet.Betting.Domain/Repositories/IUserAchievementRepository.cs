using _67Bet.Betting.Domain.Entities.Gamification;

namespace _67Bet.Betting.Domain.Repositories;

public interface IUserAchievementRepository
{
    Task<IEnumerable<UserAchievement>> GetByUserIdAsync(Guid userId);
    Task<UserAchievement?> GetSpecificAsync(Guid userId, Guid achievementId);
    Task AddAsync(UserAchievement userAchievement);
    Task UpdateAsync(UserAchievement userAchievement);
}
