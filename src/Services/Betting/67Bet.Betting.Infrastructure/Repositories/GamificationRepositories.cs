using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities.Gamification;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Infrastructure.Repositories;

public class UserGamificationRepository : EFRepository<UserGamification, BettingDbContext>, IUserGamificationRepository
{
    public UserGamificationRepository(BettingDbContext context) : base(context) { }

    public async Task<UserGamification?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.FirstOrDefaultAsync(ug => ug.UserId == userId);
    }
}

public class AchievementRepository : EFRepository<Achievement, BettingDbContext>, IAchievementRepository
{
    public AchievementRepository(BettingDbContext context) : base(context) { }

    public override async Task<IEnumerable<Achievement>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
}

public class UserAchievementRepository : EFRepository<UserAchievement, BettingDbContext>, IUserAchievementRepository
{
    public UserAchievementRepository(BettingDbContext context) : base(context) { }

    public async Task<IEnumerable<UserAchievement>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet.Where(ua => ua.UserId == userId).ToListAsync();
    }

    public async Task<UserAchievement?> GetSpecificAsync(Guid userId, Guid achievementId)
    {
        return await _dbSet.FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
    }
}
