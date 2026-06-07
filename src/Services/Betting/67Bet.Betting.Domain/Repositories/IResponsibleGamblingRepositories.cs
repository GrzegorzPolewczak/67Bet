using _67Bet.Betting.Domain.Entities.ResponsibleGambling;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Repositories;

public interface IResponsibleGamblingLimitRepository : IRepository<ResponsibleGamblingLimit>
{
    Task<ResponsibleGamblingLimit?> GetByUserAndTypeAsync(Guid userId, ResponsibleLimitType type);
    Task<IReadOnlyCollection<ResponsibleGamblingLimit>> GetByUserIdAsync(Guid userId);
}

public interface ISelfExclusionRepository : IRepository<SelfExclusion>
{
    Task<SelfExclusion?> GetActiveForUserAsync(Guid userId, DateTime nowUtc);
    Task<IReadOnlyCollection<SelfExclusion>> GetRecentForUserAsync(Guid userId, int limit);
}

public interface IResponsibleGamblingActivityRepository : IRepository<ResponsibleGamblingActivity>
{
    Task<IReadOnlyCollection<ResponsibleGamblingActivity>> GetForUserSinceAsync(Guid userId, DateTime sinceUtc);
}
