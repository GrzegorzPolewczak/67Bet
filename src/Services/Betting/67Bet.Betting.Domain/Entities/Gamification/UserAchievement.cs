using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.Gamification;

public class UserAchievement : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid AchievementId { get; private set; }
    public decimal CurrentProgress { get; private set; }
    public bool IsUnlocked { get; private set; }
    public DateTime? UnlockedAt { get; private set; }

    private UserAchievement() { }

    public UserAchievement(Guid userId, Guid achievementId)
    {
        UserId = userId;
        AchievementId = achievementId;
        CurrentProgress = 0;
        IsUnlocked = false;
    }

    public bool UpdateProgress(decimal amount, decimal threshold)
    {
        if (IsUnlocked) return false;

        CurrentProgress = amount;
        
        if (CurrentProgress >= threshold)
        {
            IsUnlocked = true;
            UnlockedAt = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    public bool AddProgress(decimal amount, decimal threshold)
    {
        return UpdateProgress(CurrentProgress + amount, threshold);
    }
}
