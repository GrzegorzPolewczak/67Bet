using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.Gamification;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class GamificationServiceTests
{
    private readonly Mock<IUserGamificationRepository> _gamificationRepoMock;
    private readonly Mock<IAchievementRepository> _achievementRepoMock;
    private readonly Mock<IUserAchievementRepository> _userAchievementRepoMock;
    private readonly Mock<ILogger<GamificationService>> _loggerMock;
    private readonly IGamificationService _service;

    public GamificationServiceTests()
    {
        _gamificationRepoMock = new Mock<IUserGamificationRepository>();
        _achievementRepoMock = new Mock<IAchievementRepository>();
        _userAchievementRepoMock = new Mock<IUserAchievementRepository>();
        _loggerMock = new Mock<ILogger<GamificationService>>();

        _service = new GamificationService(
            _gamificationRepoMock.Object,
            _achievementRepoMock.Object,
            _userAchievementRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task AwardXpForBetAsync_ShouldAddXpAndUpdateRepo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;
        var gamification = new UserGamification(userId);

        _gamificationRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(gamification);
        _achievementRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Achievement>());

        // Act
        await _service.AwardXpForBetAsync(userId, stake);

        // Assert
        Assert.Equal(100, gamification.ExperiencePoints);
        _gamificationRepoMock.Verify(r => r.UpdateAsync(gamification), Times.Once);
    }

    [Fact]
    public async Task AwardXpForWinAsync_ShouldAddXpBasedOnFormula()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;
        var odds = 3.0m;
        var gamification = new UserGamification(userId);

        _gamificationRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(gamification);
        _achievementRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Achievement>());

        // Act
        // XP = 100 * (3.0 - 1) * 0.5 = 100 * 2 * 0.5 = 100 XP
        await _service.AwardXpForWinAsync(userId, stake, odds);

        // Assert
        Assert.Equal(100, gamification.ExperiencePoints);
        _gamificationRepoMock.Verify(r => r.UpdateAsync(gamification), Times.Once);
    }

    [Fact]
    public async Task ProcessDailyLoginAsync_ShouldAddFixedXp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gamification = new UserGamification(userId);

        _gamificationRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(gamification);
        _achievementRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Achievement>());

        // Act
        await _service.ProcessDailyLoginAsync(userId);

        // Assert
        Assert.Equal(20, gamification.ExperiencePoints);
        _gamificationRepoMock.Verify(r => r.UpdateAsync(gamification), Times.Once);
    }
}
