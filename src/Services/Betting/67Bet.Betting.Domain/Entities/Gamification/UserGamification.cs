using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.Gamification;

public class UserGamification : BaseEntity
{
    public Guid UserId { get; private set; }
    public long ExperiencePoints { get; private set; }
    public int CurrentLevel { get; private set; }
    public DateTime? LastLoginDate { get; private set; }

    private UserGamification() { } // For EF Core

    public UserGamification(Guid userId)
    {
        UserId = userId;
        ExperiencePoints = 0;
        CurrentLevel = 1;
    }

    public bool AddExperience(long amount)
    {
        if (amount <= 0) return false;

        ExperiencePoints += amount;
        return CheckLevelUp();
    }

    public bool CheckLevelUp()
    {
        int nextLevel = CurrentLevel + 1;
        long requiredXp = CalculateXpForLevel(nextLevel);

        bool leveledUp = false;
        while (ExperiencePoints >= requiredXp)
        {
            CurrentLevel = nextLevel;
            leveledUp = true;
            nextLevel++;
            requiredXp = CalculateXpForLevel(nextLevel);
        }

        return leveledUp;
    }

    public long CalculateXpForLevel(int level)
    {
        if (level <= 1) return 0;
        // Formula: 100 * (Level ^ 1.5)
        return (long)(100 * Math.Pow(level, 1.5));
    }

    public bool ProcessLogin(DateTime date)
    {
        if (LastLoginDate?.Date == date.Date)
            return false;

        LastLoginDate = date;
        return true;
    }
}
