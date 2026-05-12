using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class BettingServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IMarketRepository> _marketRepositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly BettingService _bettingService;

    public BettingServiceTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _marketRepositoryMock = new Mock<IMarketRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _bettingService = new BettingService(
            _eventRepositoryMock.Object,
            _marketRepositoryMock.Object,
            _ticketRepositoryMock.Object);
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldThrowException_WhenStakeIsZero()
    {
        // Act & Assert
        await _bettingService.Invoking(s => s.PlaceTicketAsync(Guid.NewGuid(), 0, new List<Guid> { Guid.NewGuid() }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Stawka musi być większa od zera.");
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldThrowException_WhenNoOutcomesProvided()
    {
        // Act & Assert
        await _bettingService.Invoking(s => s.PlaceTicketAsync(Guid.NewGuid(), 10, new List<Guid>()))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Kupon musi zawierać przynajmniej jeden zakład.");
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldCreateTicket_WhenValidDataProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;

        var sport = new Sport("Football");
        var @event = new Event("Test Match", sport.Id, "Test League", DateTime.Now.AddDays(1));
        var market = new Market(@event.Id, "Winner");
        var outcome = new Outcome(market.Id, "Team A", 0.5m, 2.0m);
        market.Outcomes.Add(outcome);
        
        _eventRepositoryMock.Setup(x => x.GetActiveEventsAsync())
            .ReturnsAsync(new List<Event> { @event });
        
        _marketRepositoryMock.Setup(x => x.GetByEventIdAsync(@event.Id))
            .ReturnsAsync(new List<Market> { market });

        // Act
        var ticket = await _bettingService.PlaceTicketAsync(userId, stake, new List<Guid> { outcome.Id });

        // Assert
        ticket.Should().NotBeNull();
        ticket.UserId.Should().Be(userId);
        ticket.Stake.Should().Be(stake);
        ticket.Bets.Should().HaveCount(1);
        ticket.Bets.First().OutcomeId.Should().Be(outcome.Id);
        ticket.TotalOdds.Should().Be(2.0m);
        ticket.PotentialWinning.Should().Be(200m);
        
        _ticketRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Once);
    }

    [Fact]
    public async Task SettleEventAsync_ShouldUpdateEventStatusAndOutcomes()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sport = new Sport("Football");
        var @event = new Event("Test Match", sport.Id, "Test League", DateTime.Now.AddDays(1));
        // We need to set @event.Id to eventId, but it's protected set.
        // Let's use the actual ID from the created object.
        eventId = @event.Id;

        var market = new Market(eventId, "Winner");
        var outcome1 = new Outcome(market.Id, "Team A", 0.5m, 2.0m);
        var outcome2 = new Outcome(market.Id, "Team B", 0.5m, 2.0m);
        market.Outcomes.Add(outcome1);
        market.Outcomes.Add(outcome2);

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId)).ReturnsAsync(@event);
        _marketRepositoryMock.Setup(x => x.GetByEventIdAsync(eventId)).ReturnsAsync(new List<Market> { market });

        // Act
        await _bettingService.SettleEventAsync(eventId, new List<Guid> { outcome1.Id });

        // Assert
        @event.Status.Should().Be(EventStatus.Finished);
        outcome1.IsWinner.Should().BeTrue();
        outcome2.IsWinner.Should().BeFalse();
        
        _eventRepositoryMock.Verify(x => x.UpdateAsync(@event), Times.Once);
    }
}
