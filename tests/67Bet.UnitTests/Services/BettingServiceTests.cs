using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Wallet.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class BettingServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IMarketRepository> _marketRepositoryMock;
    private readonly Mock<ITicketRepository> _ticketRepositoryMock;
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly Mock<IVirtualRaceRepository> _virtualRaceRepositoryMock;
    private readonly Mock<IGamificationService> _gamificationServiceMock;
    private readonly BettingService _bettingService;

    public BettingServiceTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _marketRepositoryMock = new Mock<IMarketRepository>();
        _ticketRepositoryMock = new Mock<ITicketRepository>();
        _walletServiceMock = new Mock<IWalletService>();
        _virtualRaceRepositoryMock = new Mock<IVirtualRaceRepository>();
        _gamificationServiceMock = new Mock<IGamificationService>();
        _bettingService = new BettingService(
            _eventRepositoryMock.Object,
            _marketRepositoryMock.Object,
            _ticketRepositoryMock.Object,
            _walletServiceMock.Object,
            _virtualRaceRepositoryMock.Object,
            _gamificationServiceMock.Object);
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldThrowException_WhenStakeIsZero()
    {
        // Act & Assert
        await _bettingService.Invoking(s => s.PlaceTicketAsync(Guid.NewGuid(), 0, new List<Guid> { Guid.NewGuid() }))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldThrowException_WhenInsufficientFunds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;
        _walletServiceMock.Setup(w => w.ProcessStakeAsync(userId, stake)).ReturnsAsync(false);

        // Act & Assert
        await _bettingService.Invoking(s => s.PlaceTicketAsync(userId, stake, new List<Guid> { Guid.NewGuid() }))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PlaceTicketAsync_ShouldCreateTicket_WhenValidDataProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;
        _walletServiceMock.Setup(w => w.ProcessStakeAsync(userId, stake)).ReturnsAsync(true);

        var sportId = Guid.NewGuid();
        var @event = new Event("Test Match", sportId, "Test League", DateTime.Now.AddDays(1), "{}");
        var market = new Market(@event.Id, "Winner");
        var outcome = new Outcome(market.Id, "Team A", 0.5m, 2.0m);
        market.Outcomes.Add(outcome);
        @event.Markets.Add(market);

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
        _walletServiceMock.Verify(w => w.ProcessStakeAsync(userId, stake), Times.Once);
    }

    [Fact]
    public async Task SettleEventAsync_ShouldHandlePayout_WhenTicketIsWon()
    {
        // Arrange
        var sportId = Guid.NewGuid();
        var @event = new Event("Test Match", sportId, "Test League", DateTime.Now.AddDays(1), "{}");
        var eventId = @event.Id;

        var market = new Market(eventId, "Winner");
        var outcome = new Outcome(market.Id, "Team A", 0.5m, 2.0m);
        market.Outcomes.Add(outcome);

        var userId = Guid.NewGuid();
        var ticket = new Ticket(userId, 100m);
        ticket.AddBet(outcome.Id, outcome.Name, market.Name, @event.Name, @event.StartTime, 2.0m);
        ticket.Settle(TicketStatus.Pending);

        _eventRepositoryMock.Setup(x => x.GetByIdAsync(eventId)).ReturnsAsync(@event);
        _marketRepositoryMock.Setup(x => x.GetByEventIdAsync(eventId)).ReturnsAsync(new List<Market> { market });
        _marketRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Market> { market }); // Added this
        _ticketRepositoryMock.Setup(x => x.GetActiveTicketsAsync()).ReturnsAsync(new List<Ticket> { ticket });
        _eventRepositoryMock.Setup(x => x.GetActiveEventsAsync()).ReturnsAsync(new List<Event> { @event });

        // Ensure outcome is winner
        outcome.SetResult(true);

        // Act
        await _bettingService.SettleEventAsync(eventId, new List<Guid> { outcome.Id });

        // Assert
        ticket.Status.Should().Be(TicketStatus.Won);
        _walletServiceMock.Verify(w => w.ProcessPayoutAsync(userId, 200m), Times.Once);
        _ticketRepositoryMock.Verify(x => x.UpdateAsync(ticket), Times.AtLeastOnce);
    }

    [Fact]
    public void Ticket_CalculatePotentialWinning_ShouldApply70PercentMultiplier_WhenIsFreebet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stake = 100m;
        var ticket = new Ticket(userId, stake, isFreebet: true);

        // Act
        ticket.AddBet(Guid.NewGuid(), "Outcome", "Market", "Event", DateTime.Now, 2.0m); // Odds 2.0
        
        // Assert
        // Standard win: 100 * 2.0 = 200
        // Freebet win: 200 * 0.7 = 140
        ticket.PotentialWinning.Should().Be(140m);
    }
}
