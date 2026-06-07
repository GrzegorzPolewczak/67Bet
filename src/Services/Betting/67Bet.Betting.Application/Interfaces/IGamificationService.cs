using _67Bet.Betting.Application.DTOs;

namespace _67Bet.Betting.Application.Interfaces;

public interface IGamificationService
{
    Task AwardXpForBetAsync(Guid userId, decimal stake);
    Task AwardXpForWinAsync(Guid userId, decimal stake, decimal odds);
    Task ProcessDailyLoginAsync(Guid userId);
    Task<UserGamificationDto> GetUserProgressAsync(Guid userId);
    Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(Guid userId);
}
