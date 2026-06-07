using Moq;
using Xunit;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace _67Bet.UnitTests.Services;

public class AiAssistantServiceTests
{
    private readonly Mock<IAiMatchInsightRepository> _insightRepoMock;
    private readonly Mock<IEventRepository> _eventRepoMock;
    private readonly Mock<IOddsServiceClient> _oddsServiceMock;
    private readonly Mock<IGeminiClient> _geminiClientMock;
    private readonly Mock<ILogger<AiAssistantService>> _loggerMock;
    private readonly AiAssistantService _service;

    public AiAssistantServiceTests()
    {
        _insightRepoMock = new Mock<IAiMatchInsightRepository>();
        _eventRepoMock = new Mock<IEventRepository>();
        _oddsServiceMock = new Mock<IOddsServiceClient>();
        _geminiClientMock = new Mock<IGeminiClient>();
        _loggerMock = new Mock<ILogger<AiAssistantService>>();

        _service = new AiAssistantService(
            _insightRepoMock.Object,
            _eventRepoMock.Object,
            _oddsServiceMock.Object,
            _geminiClientMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetMatchInsightAsync_ShouldReturnCachedValue_WhenExistsInDbAndIsRecent()
    {
        // Arrange
        var eventGuid = Guid.NewGuid();
        var eventId = eventGuid.ToString();
        var cachedContent = "To jest tekst z bazy";
        var existingInsight = new AiMatchInsight(eventId, cachedContent);

        _insightRepoMock.Setup(r => r.GetByEventIdAsync(eventId))
            .ReturnsAsync(existingInsight);

        // Act
        var result = await _service.GetMatchInsightAsync(eventId);

        // Assert
        Assert.Equal(cachedContent, result);
        _geminiClientMock.Verify(c => c.GenerateTextAsync(It.IsAny<string>()), Times.Never);
        _insightRepoMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<AiMatchInsight>()), Times.Never);
    }

    [Fact]
    public async Task GetMatchInsightAsync_ShouldCallGeminiAndSaveToDbAndLog_WhenNotCached()
    {
        // Arrange
        var eventGuid = Guid.NewGuid();
        var eventId = eventGuid.ToString();
        var matchName = "Team A vs Team B";
        var match = new Event(matchName, Guid.NewGuid(), "Champions League", DateTime.UtcNow, "Football");
        var aiResponse = "Analiza od AI";

        _insightRepoMock.Setup(r => r.GetByEventIdAsync(eventId))
            .ReturnsAsync((AiMatchInsight?)null);

        _eventRepoMock.Setup(r => r.GetByIdAsync(eventGuid))
            .ReturnsAsync(match);

        _geminiClientMock.Setup(c => c.GenerateTextAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GetMatchInsightAsync(eventId);

        // Assert
        Assert.Equal(aiResponse, result);
        _geminiClientMock.Verify(c => c.GenerateTextAsync(It.IsAny<string>()), Times.Once);
        _insightRepoMock.Verify(r => r.AddOrUpdateAsync(It.Is<AiMatchInsight>(i => i.Content == aiResponse && i.EventId == eventId)), Times.Once);
        _insightRepoMock.Verify(r => r.AddLogAsync(It.Is<AiGenerationLog>(l => l.EventId == eventId && l.Status == "Success")), Times.Once);
    }

    [Fact]
    public async Task GetAllInsightsAsync_ShouldReturnAllInsights()
    {
        // Arrange
        var insights = new List<AiMatchInsight>
        {
            new AiMatchInsight("event1", "content1"),
            new AiMatchInsight("event2", "content2")
        };
        _insightRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(insights);

        // Act
        var result = await _service.GetAllInsightsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _insightRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task RegenerateInsightAsync_ShouldForceCallGemini()
    {
        // Arrange
        var eventId = "event123";
        var externalMatch = new ExternalMatchDto { Name = "Team A vs Team B", SportKey = "Soccer" };
        _oddsServiceMock.Setup(s => s.GetEventByIdAsync(eventId)).ReturnsAsync(externalMatch);
        _geminiClientMock.Setup(c => c.GenerateTextAsync(It.IsAny<string>())).ReturnsAsync("New Analysis");

        // Act
        var result = await _service.RegenerateInsightAsync(eventId);

        // Assert
        Assert.Equal("New Analysis", result);
        _geminiClientMock.Verify(c => c.GenerateTextAsync(It.IsAny<string>()), Times.Once);
        _insightRepoMock.Verify(r => r.AddOrUpdateAsync(It.IsAny<AiMatchInsight>()), Times.Once);
    }

    [Fact]
    public async Task DeleteInsightAsync_ShouldCallRepositoryDelete()
    {
        // Arrange
        var eventId = "event123";

        // Act
        var result = await _service.DeleteInsightAsync(eventId);

        // Assert
        Assert.True(result);
        _insightRepoMock.Verify(r => r.DeleteAsync(eventId), Times.Once);
    }

    [Fact]
    public async Task GetMatchInsightAsync_ShouldCallOddsService_WhenNotFoundLocally()
    {
        // Arrange
        var eventGuid = Guid.NewGuid();
        var eventId = eventGuid.ToString();
        var externalMatch = new ExternalMatchDto { Name = "External Team A vs B", SportKey = "Tennis", RecentScores = "scores", CurrentOdds = "odds" };
        var aiResponse = "Analiza od AI dla meczu zewnętrznego";

        _insightRepoMock.Setup(r => r.GetByEventIdAsync(eventId))
            .ReturnsAsync((AiMatchInsight?)null);

        _eventRepoMock.Setup(r => r.GetByIdAsync(eventGuid))
            .ReturnsAsync((Event?)null);

        _oddsServiceMock.Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync(externalMatch);

        _geminiClientMock.Setup(c => c.GenerateTextAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GetMatchInsightAsync(eventId);

        // Assert
        Assert.Equal(aiResponse, result);
        _oddsServiceMock.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
        _geminiClientMock.Verify(c => c.GenerateTextAsync(It.Is<string>(p => p.Contains(externalMatch.Name) && p.Contains("scores") && p.Contains("odds"))), Times.Once);
    }

    [Fact]
    public async Task GetMatchInsightAsync_ShouldHandleNonGuidId()
    {
        // Arrange
        var eventId = "external_id_123";
        var externalMatch = new ExternalMatchDto { Name = "External Team A vs B", SportKey = "Tennis" };
        var aiResponse = "Analiza od AI dla meczu zewnętrznego";

        _oddsServiceMock.Setup(s => s.GetEventByIdAsync(eventId))
            .ReturnsAsync(externalMatch);

        _geminiClientMock.Setup(c => c.GenerateTextAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GetMatchInsightAsync(eventId);

        // Assert
        Assert.Equal(aiResponse, result);
        _oddsServiceMock.Verify(s => s.GetEventByIdAsync(eventId), Times.Once);
        _eventRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }
}
