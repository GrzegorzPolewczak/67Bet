namespace _67Bet.Betting.Application.DTOs;

public record UserGamificationDto(
    Guid UserId,
    long ExperiencePoints,
    int CurrentLevel,
    long NextLevelXp,
    double ProgressPercentage
);

public record AchievementDto(
    Guid Id,
    string Name,
    string Description,
    string Type,
    decimal Threshold,
    string IconUrl
);

public record UserAchievementDto(
    Guid AchievementId,
    string Name,
    string Description,
    decimal CurrentProgress,
    decimal Threshold,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    string IconUrl,
    string Type
);

public record GamificationUpdateNotification(
    long NewXp,
    int CurrentLevel,
    bool LeveledUp,
    List<string> UnlockedAchievements
);
