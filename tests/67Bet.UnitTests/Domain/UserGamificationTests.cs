using _67Bet.Betting.Domain.Entities.Gamification;
using Xunit;

namespace _67Bet.UnitTests.Domain;

public class UserGamificationTests
{
    [Fact]
    public void AddExperience_ShouldIncreaseXp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);

        // Act
        gamification.AddExperience(50);

        // Assert
        Assert.Equal(50, gamification.ExperiencePoints);
    }

    [Fact]
    public void AddExperience_ShouldLevelUp_WhenThresholdReached()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);

        // Act
        // Level 2 requires 100 * (2^1.5) = 100 * 2.828 = 282 XP
        gamification.AddExperience(300);

        // Assert
        Assert.Equal(2, gamification.CurrentLevel);
        Assert.Equal(300, gamification.ExperiencePoints);
    }

    [Fact]
    public void AddExperience_ShouldLevelUpMultipleTimes_WhenLargeXpAdded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);

        // Act
        // Level 3 requires 100 * (3^1.5) = 100 * 5.196 = 519 XP
        gamification.AddExperience(600);

        // Assert
        Assert.Equal(3, gamification.CurrentLevel);
    }

    [Fact]
    public void ProcessLogin_ShouldReturnTrue_ForFirstLoginOfDay()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);
        var loginDate = DateTime.UtcNow;

        // Act
        var result = gamification.ProcessLogin(loginDate);

        // Assert
        Assert.True(result);
        Assert.Equal(loginDate, gamification.LastLoginDate);
    }

    [Fact]
    public void ProcessLogin_ShouldReturnFalse_ForSecondLoginSameDay()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);
        var loginDate = DateTime.UtcNow;

        // Act
        gamification.ProcessLogin(loginDate);
        var result = gamification.ProcessLogin(loginDate.AddHours(1));

        // Assert
        Assert.False(result);
    }
}
