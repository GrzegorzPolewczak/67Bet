using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Application.Services;
using _67Bet.Odds.Domain.Entities;
using _67Bet.Odds.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class OddsIntegrationServiceTests
{
    private readonly Mock<ITheOddsApiClient> _apiClientMock;
    private readonly Mock<IPandaScoreApiClient> _pandaApiClientMock;
    private readonly Mock<IExternalEventRepository> _eventRepositoryMock;
    private readonly OddsIntegrationService _service;

    public OddsIntegrationServiceTests()
    {
        _apiClientMock = new Mock<ITheOddsApiClient>();
        _pandaApiClientMock = new Mock<IPandaScoreApiClient>();
        _eventRepositoryMock = new Mock<IExternalEventRepository>();
        _service = new OddsIntegrationService(
            _apiClientMock.Object,
            _pandaApiClientMock.Object,
            _eventRepositoryMock.Object,
            new NullLogger<OddsIntegrationService>());
    }

    [Fact]
    public async Task SyncExternalOddsAsync_ShouldAddNewEvents_WhenTheyDoNotExist()
    {
        // Arrange
        var externalEvents = new List<ExternalEventDto>
        {
            new ExternalEventDto
            {
                Id = "ext1",
                SportKey = "soccer",
                HomeTeam = "Team A",
                AwayTeam = "Team B",
                CommenceTime = DateTime.UtcNow.AddDays(1),
                Bookmakers = new List<BookmakerDto>
                {
                    new BookmakerDto
                    {
                        Key = "bm1",
                        Markets = new List<ExternalMarketDto>
                        {
                            new ExternalMarketDto
                            {
                                Key = "h2h",
                                Outcomes = new List<ExternalOutcomeDto>
                                {
                                    new ExternalOutcomeDto { Name = "Team A", Price = 2.0m },
                                    new ExternalOutcomeDto { Name = "Team B", Price = 3.0m }
                                }
                            }
                        }
                    }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetUpcomingEventsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ExternalEventDto>());

        _apiClientMock.Setup(x => x.GetUpcomingEventsAsync("upcoming", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(externalEvents);
        
        _pandaApiClientMock.Setup(x => x.GetUpcomingEsportsMatchesAsync())
            .ReturnsAsync(new List<ExternalEventDto>());

        _eventRepositoryMock.Setup(x => x.GetByExternalIdAsync("ext1"))
            .ReturnsAsync((ExternalEvent?)null);

        // Act
        var result = await _service.SyncExternalOddsAsync();

        // Assert
        Assert.Equal(1, result.EventsProcessed);
        Assert.Equal(1, result.NewEventsAdded);
        _eventRepositoryMock.Verify(x => x.AddAsync(It.Is<ExternalEvent>(e => e.ExternalId == "ext1")), Times.Once);
    }
}
