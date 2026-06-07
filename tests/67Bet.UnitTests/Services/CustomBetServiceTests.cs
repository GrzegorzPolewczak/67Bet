using System;
using System.Threading.Tasks;
using _67Bet.CustomBet.Application.Services;
using _67Bet.CustomBet.Domain.Entities;
using _67Bet.CustomBet.Domain.Enums;
using _67Bet.CustomBet.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

using _67Bet.CustomBet.Application.Interfaces;

namespace _67Bet.UnitTests.Services;

public class CustomBetServiceTests
{
    private readonly Mock<ICustomBetRepository> _customBetRepositoryMock;
    private readonly Mock<IGeminiClient> _geminiClientMock;
    private readonly CustomBetService _customBetService;

    public CustomBetServiceTests()
    {
        _customBetRepositoryMock = new Mock<ICustomBetRepository>();
        _geminiClientMock = new Mock<IGeminiClient>();
        _customBetService = new CustomBetService(_customBetRepositoryMock.Object, _geminiClientMock.Object);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var description = "Will it rain during the match?";

        // Act
        var request = await _customBetService.CreateRequestAsync(userId, description);

        // Assert
        request.Should().NotBeNull();
        request.UserId.Should().Be(userId);
        request.Description.Should().Be(description);
        request.Status.Should().Be(RequestStatus.Pending);

        _customBetRepositoryMock.Verify(x => x.AddAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetAiRecommendationAsync_ShouldUpdateFieldsWithAiData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CustomBetRequest(userId, "Barcelona to win 5-0");
        var requestId = request.Id;
        var aiResponse = "{ \"odds\": 15.5, \"risk\": \"High\", \"reasoning\": \"Highly unlikely scoreline\", \"category\": \"Football\" }";

        _customBetRepositoryMock.Setup(x => x.GetByIdAsync(requestId)).ReturnsAsync(request);
        _geminiClientMock.Setup(x => x.GenerateTextAsync(It.IsAny<string>())).ReturnsAsync(aiResponse);

        // Act
        var result = await _customBetService.GetAiRecommendationAsync(requestId);

        // Assert
        result.AiSuggestedOdds.Should().Be(15.5m);
        result.AiRiskLevel.Should().Be("High");
        result.AiAnalysisNote.Should().Be("Highly unlikely scoreline");
        result.AiCategory.Should().Be("Football");
        result.Status.Should().Be(RequestStatus.Reviewing);

        _customBetRepositoryMock.Verify(x => x.UpdateAsync(request), Times.Once);
    }

    [Fact]
    public async Task AcceptRequestAsync_ShouldSetAcceptedStatusAndFinalOdds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CustomBetRequest(userId, "Test");
        var requestId = request.Id;

        _customBetRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act
        await _customBetService.AcceptRequestAsync(requestId, 3.0m, "Looks good");

        // Assert
        request.Status.Should().Be(RequestStatus.Accepted);
        request.AdminFinalOdds.Should().Be(3.0m);
        request.AdminNote.Should().Be("Looks good");

        _customBetRepositoryMock.Verify(x => x.UpdateAsync(request), Times.Once);
    }

    [Fact]
    public async Task RejectRequestAsync_ShouldSetRejectedStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CustomBetRequest(userId, "Test");
        var requestId = request.Id;

        _customBetRepositoryMock.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        // Act
        await _customBetService.RejectRequestAsync(requestId, "Invalid market");

        // Assert
        request.Status.Should().Be(RequestStatus.Rejected);
        request.AdminNote.Should().Be("Invalid market");

        _customBetRepositoryMock.Verify(x => x.UpdateAsync(request), Times.Once);
    }
}
