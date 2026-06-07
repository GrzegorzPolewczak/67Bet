using _67Bet.Betting.Domain.Entities.ResponsibleGambling;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;
using Microsoft.EntityFrameworkCore;

namespace _67Bet.Betting.Infrastructure.Repositories;

public sealed class ResponsibleGamblingLimitRepository
    : EFRepository<ResponsibleGamblingLimit, BettingDbContext>, IResponsibleGamblingLimitRepository
{
    public ResponsibleGamblingLimitRepository(BettingDbContext context) : base(context)
    {
    }

    public async Task<ResponsibleGamblingLimit?> GetByUserAndTypeAsync(Guid userId, ResponsibleLimitType type)
    {
        return await _dbSet.FirstOrDefaultAsync(limit => limit.UserId == userId && limit.Type == type);
    }

    public async Task<IReadOnlyCollection<ResponsibleGamblingLimit>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(limit => limit.UserId == userId)
            .OrderBy(limit => limit.Type)
            .ToArrayAsync();
    }
}

public sealed class SelfExclusionRepository
    : EFRepository<SelfExclusion, BettingDbContext>, ISelfExclusionRepository
{
    public SelfExclusionRepository(BettingDbContext context) : base(context)
    {
    }

    public async Task<SelfExclusion?> GetActiveForUserAsync(Guid userId, DateTime nowUtc)
    {
        return await _dbSet
            .Where(exclusion => exclusion.UserId == userId &&
                                exclusion.StartsAtUtc <= nowUtc &&
                                exclusion.EndsAtUtc > nowUtc)
            .OrderByDescending(exclusion => exclusion.EndsAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<SelfExclusion>> GetRecentForUserAsync(Guid userId, int limit)
    {
        return await _dbSet
            .Where(exclusion => exclusion.UserId == userId)
            .OrderByDescending(exclusion => exclusion.StartsAtUtc)
            .Take(Math.Clamp(limit, 1, 50))
            .ToArrayAsync();
    }
}

public sealed class ResponsibleGamblingActivityRepository
    : EFRepository<ResponsibleGamblingActivity, BettingDbContext>, IResponsibleGamblingActivityRepository
{
    public ResponsibleGamblingActivityRepository(BettingDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyCollection<ResponsibleGamblingActivity>> GetForUserSinceAsync(Guid userId, DateTime sinceUtc)
    {
        return await _dbSet
            .Where(activity => activity.UserId == userId && activity.OccurredAtUtc >= sinceUtc)
            .OrderByDescending(activity => activity.OccurredAtUtc)
            .ToArrayAsync();
    }
}
