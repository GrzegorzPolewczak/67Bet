using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.Roulette;
using _67Bet.Betting.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace _67Bet.UnitTests.Services;

public class RouletteServiceTests
{
    [Fact]
    public async Task PlayAsync_WhenStakeAccepted_CreatesRoundAndReturnsResult()
    {
        var repo = new FakeRouletteRoundRepository();
        var wallet = new FakeRouletteWalletGateway(true);
        var service = new RouletteService(repo, wallet, new Mock<IGamificationService>().Object);

        var result = await service.PlayAsync(Guid.NewGuid(),
            new RoulettePlayRequest(new[] { new RouletteBetRequest(RouletteBetType.Red, null, 10m) }),
            "token");

        result.SpinResult.Should().BeInRange(0, 36);
        result.TotalStake.Should().Be(10m);
        result.IsPayoutSettled.Should().BeFalse();
        repo.Rounds.Should().ContainSingle();
        wallet.ProcessedStake.Should().Be(10m);
    }

    [Fact]
    public async Task PlayAsync_WithMultipleBets_ChargesCorrectTotalStake()
    {
        var repo = new FakeRouletteRoundRepository();
        var wallet = new FakeRouletteWalletGateway(true);
        var service = new RouletteService(repo, wallet, new Mock<IGamificationService>().Object);

        var result = await service.PlayAsync(Guid.NewGuid(), new RoulettePlayRequest(new[]
        {
            new RouletteBetRequest(RouletteBetType.Red, null, 10m),
            new RouletteBetRequest(RouletteBetType.Even, null, 5m),
            new RouletteBetRequest(RouletteBetType.StraightUp, 17, 2m)
        }), "token");

        result.TotalStake.Should().Be(17m);
        wallet.ProcessedStake.Should().Be(17m);
        result.Bets.Should().HaveCount(3);
    }

    [Fact]
    public async Task PlayAsync_WhenWalletRejectsStake_ThrowsAndDoesNotStoreRound()
    {
        var repo = new FakeRouletteRoundRepository();
        var service = new RouletteService(repo, new FakeRouletteWalletGateway(false), new Mock<IGamificationService>().Object);

        var act = () => service.PlayAsync(Guid.NewGuid(),
            new RoulettePlayRequest(new[] { new RouletteBetRequest(RouletteBetType.Black, null, 20m) }),
            null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient wallet balance.");
        repo.Rounds.Should().BeEmpty();
    }

    [Fact]
    public async Task PlayAsync_WhenNoBetsProvided_Throws()
    {
        var service = new RouletteService(new FakeRouletteRoundRepository(), new FakeRouletteWalletGateway(true), new Mock<IGamificationService>().Object);

        var act = () => service.PlayAsync(Guid.NewGuid(),
            new RoulettePlayRequest(Array.Empty<RouletteBetRequest>()),
            "token");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("At least one bet is required.");
    }

    [Fact]
    public async Task SettleRoundAsync_WhenRoundExists_ProcessesPayoutOnce()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeRouletteRoundRepository();
        var wallet = new FakeRouletteWalletGateway(true);
        var service = new RouletteService(repo, wallet, new Mock<IGamificationService>().Object);
        var round = await service.PlayAsync(userId,
            new RoulettePlayRequest(new[] { new RouletteBetRequest(RouletteBetType.Red, null, 10m) }),
            "token");

        var settled = await service.SettleRoundAsync(userId, round.Id, "token");
        var settledAgain = await service.SettleRoundAsync(userId, round.Id, "token");

        settled.IsPayoutSettled.Should().BeTrue();
        settledAgain.IsPayoutSettled.Should().BeTrue();
        wallet.PayoutCallCount.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsOnlyUserRounds()
    {
        var userId = Guid.NewGuid();
        var repo = new FakeRouletteRoundRepository();
        var wallet = new FakeRouletteWalletGateway(true);
        var service = new RouletteService(repo, wallet, new Mock<IGamificationService>().Object);
        await service.PlayAsync(userId,
            new RoulettePlayRequest(new[] { new RouletteBetRequest(RouletteBetType.Red, null, 5m) }), "token");
        await service.PlayAsync(Guid.NewGuid(),
            new RoulettePlayRequest(new[] { new RouletteBetRequest(RouletteBetType.Black, null, 5m) }), "token");

        var history = await service.GetHistoryAsync(userId, 10);

        history.Should().ContainSingle();
        history.Single().TotalStake.Should().Be(5m);
    }

    [Fact]
    public void RouletteRound_StraightUpBet_WinsWhenNumberMatches()
    {
        var bet = new RouletteBet(RouletteBetType.StraightUp, 17, 5m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 17);

        round.Bets.Single().IsWon.Should().BeTrue();
        round.Bets.Single().Payout.Should().Be(180m);
    }

    [Fact]
    public void RouletteRound_StraightUpBet_LosesWhenNumberDoesNotMatch()
    {
        var bet = new RouletteBet(RouletteBetType.StraightUp, 17, 5m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 5);

        round.Bets.Single().IsWon.Should().BeFalse();
        round.Bets.Single().Payout.Should().Be(0m);
    }

    [Fact]
    public void RouletteRound_RedBet_WinsOnRedNumber()
    {
        var bet = new RouletteBet(RouletteBetType.Red, null, 10m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 1);

        round.Bets.Single().IsWon.Should().BeTrue();
        round.Bets.Single().Payout.Should().Be(20m);
    }

    [Fact]
    public void RouletteRound_RedBet_LosesOnZero()
    {
        var bet = new RouletteBet(RouletteBetType.Red, null, 10m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 0);

        round.Bets.Single().IsWon.Should().BeFalse();
        round.Bets.Single().Payout.Should().Be(0m);
    }

    [Fact]
    public void RouletteRound_EvenBet_LosesOnZero()
    {
        var bet = new RouletteBet(RouletteBetType.Even, null, 10m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 0);

        round.Bets.Single().IsWon.Should().BeFalse();
    }

    [Fact]
    public void RouletteRound_DozenFirstBet_WinsOnNumbers1To12()
    {
        var bet = new RouletteBet(RouletteBetType.DozenFirst, null, 10m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet }, 7);

        round.Bets.Single().IsWon.Should().BeTrue();
        round.Bets.Single().Payout.Should().Be(30m);
    }

    [Fact]
    public void RouletteRound_TotalPayout_SumsWinningBets()
    {
        var bet1 = new RouletteBet(RouletteBetType.Red, null, 10m);
        var bet2 = new RouletteBet(RouletteBetType.Odd, null, 5m);
        var round = new RouletteRound(Guid.NewGuid(), new[] { bet1, bet2 }, 1);

        round.TotalPayout.Should().Be(30m);
    }

    private sealed class FakeRouletteRoundRepository : IRouletteRoundRepository
    {
        public List<RouletteRound> Rounds { get; } = new();

        public Task AddAsync(RouletteRound round) { Rounds.Add(round); return Task.CompletedTask; }
        public Task<RouletteRound?> GetByIdAsync(Guid roundId) => Task.FromResult(Rounds.SingleOrDefault(r => r.Id == roundId));
        public Task UpdateAsync(RouletteRound round) => Task.CompletedTask;

        public Task<IReadOnlyCollection<RouletteRound>> GetRecentForUserAsync(Guid userId, int limit)
        {
            var result = Rounds
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Take(limit)
                .ToArray();
            return Task.FromResult<IReadOnlyCollection<RouletteRound>>(result);
        }
    }

    private sealed class FakeRouletteWalletGateway : IRouletteWalletGateway
    {
        private readonly bool _stakeAccepted;
        public decimal ProcessedStake { get; private set; }
        public decimal ProcessedPayout { get; private set; }
        public int PayoutCallCount { get; private set; }

        public FakeRouletteWalletGateway(bool stakeAccepted) { _stakeAccepted = stakeAccepted; }

        public Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken)
        {
            ProcessedStake = amount;
            return Task.FromResult(_stakeAccepted);
        }

        public Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken)
        {
            ProcessedPayout = amount;
            PayoutCallCount++;
            return Task.CompletedTask;
        }
    }
}
