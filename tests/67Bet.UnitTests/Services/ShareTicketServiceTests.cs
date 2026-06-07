using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Mappings;
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

public class ShareTicketServiceTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock = new();
    private readonly Mock<IMarketRepository> _marketRepositoryMock = new();
    private readonly Mock<ISportRepository> _sportRepositoryMock = new();
    private readonly Mock<ITicketRepository> _ticketRepositoryMock = new();
    private readonly Mock<IWalletService> _walletServiceMock = new();
    private readonly Mock<IVirtualRaceRepository> _virtualRaceRepositoryMock = new();
    private readonly Mock<IGamificationService> _gamificationServiceMock = new();
    private readonly Mock<IResponsibleGamblingService> _responsibleGamblingServiceMock = new();
    private readonly Mock<IOddsServiceClient> _oddsServiceClientMock = new();

    private BettingService CreateService()
    {
        _responsibleGamblingServiceMock
            .Setup(x => x.ValidateStakeAsync(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync(new ResponsibleGamblingValidationResultDto(true, null, null, null, null, null));

        return new BettingService(
            _eventRepositoryMock.Object,
            _marketRepositoryMock.Object,
            _sportRepositoryMock.Object,
            _ticketRepositoryMock.Object,
            _walletServiceMock.Object,
            _virtualRaceRepositoryMock.Object,
            _gamificationServiceMock.Object,
            _responsibleGamblingServiceMock.Object,
            _oddsServiceClientMock.Object);
    }

    [Fact]
    public async Task GetTicketByIdAsync_ShouldReturnTicketWithBets_WhenSharedTicketExists()
    {
        var userId = Guid.NewGuid();
        var ticket = new Ticket(userId, 50m);
        ticket.AddBet(
            Guid.NewGuid(),
            "Team A",
            "Winner",
            "Team A vs Team B",
            DateTime.UtcNow.AddHours(2),
            2.10m);

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticket.Id))
            .ReturnsAsync(ticket);

        var service = CreateService();

        var result = await service.GetTicketByIdAsync(ticket.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ticket.Id);
        result.Bets.Should().ContainSingle();
        result.Bets.First().OutcomeName.Should().Be("Team A");
        _ticketRepositoryMock.Verify(x => x.GetByIdAsync(ticket.Id), Times.Once);
    }

    [Fact]
    public async Task GetTicketByIdAsync_ShouldReturnNull_WhenSharedTicketDoesNotExist()
    {
        var ticketId = Guid.NewGuid();

        _ticketRepositoryMock
            .Setup(x => x.GetByIdAsync(ticketId))
            .ReturnsAsync((Ticket?)null);

        var service = CreateService();

        var result = await service.GetTicketByIdAsync(ticketId);

        result.Should().BeNull();
        _ticketRepositoryMock.Verify(x => x.GetByIdAsync(ticketId), Times.Once);
    }

    [Fact]
    public void ToDto_ShouldExposeSharedTicketFields_WhenTicketIsSettled()
    {
        var ticket = new Ticket(Guid.NewGuid(), 25m);
        var outcomeId = Guid.NewGuid();

        ticket.AddBet(
            outcomeId,
            "Team A",
            "Winner",
            "Team A vs Team B",
            DateTime.UtcNow.AddHours(-1),
            1.80m);

        ticket.Bets.First().Settle(BetStatus.Won, "Team A");
        ticket.Settle(TicketStatus.Won);

        var dto = ticket.ToDto();

        dto.Id.Should().Be(ticket.Id);
        dto.Status.Should().Be("Won");
        dto.Bets.Should().ContainSingle();

        var betDto = dto.Bets.First();
        betDto.OutcomeId.Should().Be(outcomeId);
        betDto.OutcomeName.Should().Be("Team A");
        betDto.MarketName.Should().Be("Winner");
        betDto.EventName.Should().Be("Team A vs Team B");
        betDto.FixedPrice.Should().Be(1.80m);
        betDto.Status.Should().Be("Won");
        betDto.ResultStatus.Should().Be("Won");
        betDto.WinningOutcomeName.Should().Be("Team A");
    }
}
