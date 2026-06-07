using _67Bet.Shared.Kernel;
using _67Bet.Betting.Domain.Enums;

namespace _67Bet.Betting.Domain.Entities.Gamification;

public class Achievement : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public AchievementType Type { get; private set; }
    public decimal Threshold { get; private set; }
    public string IconUrl { get; private set; } = string.Empty;

    private Achievement() { }

    public Achievement(string name, string description, AchievementType type, decimal threshold, string iconUrl)
    {
        Name = name;
        Description = description;
        Type = type;
        Threshold = threshold;
        IconUrl = iconUrl;
    }
}
