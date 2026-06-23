using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.Plinko;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using FluentAssertions;
using Moq;

namespace _67Bet.UnitTests.Services;

public class PlinkoServiceTests
{
    [Fact]
    public void GetBoard_ReturnsRowsPlusOneMultipliers()
    {
        var service = CreateService();

        var board = service.GetBoard(PlinkoRiskLevel.Medium, 12);

        board.Multipliers.Should().HaveCount(13);
        board.Multipliers.First().Should().BeGreaterThan(board.Multipliers[6]);
        board.Multipliers.Last().Should().BeGreaterThan(board.Multipliers[6]);
    }

    [Fact]
    public void GetBoard_UsesReducedTopMultipliers()
    {
        var service = CreateService();

        var lowRisk = service.GetBoard(PlinkoRiskLevel.Low, 16);
        var mediumRisk = service.GetBoard(PlinkoRiskLevel.Medium, 16);
        var highRisk = service.GetBoard(PlinkoRiskLevel.High, 16);

        lowRisk.Multipliers.Max().Should().BeLessThanOrEqualTo(1.72m);
        mediumRisk.Multipliers.Max().Should().BeLessThanOrEqualTo(3.72m);
        highRisk.Multipliers.Max().Should().BeLessThanOrEqualTo(14.00m);
    }

    [Fact]
    public async Task PlayAsync_WhenStakeAccepted_CreatesRoundWithValidPayout()
    {
        var repository = new FakePlinkoRoundRepository();
        var wallet = new FakePlinkoWalletGateway(true);
        var service = new PlinkoService(repository, wallet, new Mock<IGamificationService>().Object);

        var result = await service.PlayAsync(Guid.NewGuid(), new PlinkoPlayRequest(25m, PlinkoRiskLevel.Low, 10), "token");

        result.Stake.Should().Be(25m);
        result.Path.Should().HaveLength(10);
        result.LandingSlot.Should().BeInRange(0, 10);
        result.Payout.Should().Be(Math.Round(result.Stake * result.Multiplier, 2));
        repository.Rounds.Should().ContainSingle();
        wallet.ProcessedStake.Should().Be(25m);
        wallet.ProcessedPayout.Should().Be(0m);
        result.IsPayoutSettled.Should().BeFalse();
    }

    [Fact]
    public async Task PlayAsync_WhenBoardFeeProvided_ChargesStakeAndFeeButPaysFromStake()
    {
        var repository = new FakePlinkoRoundRepository();
        var wallet = new FakePlinkoWalletGateway(true);
        var service = new PlinkoService(repository, wallet, new Mock<IGamificationService>().Object);

        var result = await service.PlayAsync(Guid.NewGuid(), new PlinkoPlayRequest(25m, PlinkoRiskLevel.Low, 10, 5m), "token");

        wallet.ProcessedStake.Should().Be(30m);
        result.Payout.Should().Be(Math.Round(result.Stake * result.Multiplier, 2));
        wallet.ProcessedPayout.Should().Be(0m);
        result.IsPayoutSettled.Should().BeFalse();
    }

    [Fact]
    public async Task SettleRoundAsync_WhenRoundExists_ProcessesPayoutOnce()
    {
        var userId = Guid.NewGuid();
        var repository = new FakePlinkoRoundRepository();
        var wallet = new FakePlinkoWalletGateway(true);
        var service = new PlinkoService(repository, wallet, new Mock<IGamificationService>().Object);
        var round = await service.PlayAsync(userId, new PlinkoPlayRequest(25m, PlinkoRiskLevel.Low, 10), "token");

        var settled = await service.SettleRoundAsync(userId, round.Id, "token");
        var settledAgain = await service.SettleRoundAsync(userId, round.Id, "token");

        settled.IsPayoutSettled.Should().BeTrue();
        settledAgain.IsPayoutSettled.Should().BeTrue();
        wallet.ProcessedPayout.Should().Be(round.Payout);
        wallet.PayoutCallCount.Should().Be(1);
    }

    [Fact]
    public async Task PlayAndSettle_WhenResponsibleGamblingServiceProvided_RecordsStakeAndPayout()
    {
        var userId = Guid.NewGuid();
        var repository = new FakePlinkoRoundRepository();
        var wallet = new FakePlinkoWalletGateway(true);
        var responsibleGambling = new FakeResponsibleGamblingService();
        var service = new PlinkoService(repository, wallet, new Mock<IGamificationService>().Object, responsibleGambling);

        var round = await service.PlayAsync(userId, new PlinkoPlayRequest(25m, PlinkoRiskLevel.Low, 10, 5m), "token");
        await service.SettleRoundAsync(userId, round.Id, "token");

        responsibleGambling.ValidatedStake.Should().Be(30m);
        responsibleGambling.Activities.Should().Contain(activity =>
            activity.Type == ResponsibleGamblingActivityType.Stake && activity.Amount == 30m);
        responsibleGambling.Activities.Should().Contain(activity =>
            activity.Type == ResponsibleGamblingActivityType.Payout && activity.Amount == round.Payout);
    }

    [Fact]
    public async Task PlayAsync_WhenWalletRejectsStake_ThrowsAndDoesNotStoreRound()
    {
        var repository = new FakePlinkoRoundRepository();
        var service = new PlinkoService(repository, new FakePlinkoWalletGateway(false), new Mock<IGamificationService>().Object);

        var act = () => service.PlayAsync(Guid.NewGuid(), new PlinkoPlayRequest(100m, PlinkoRiskLevel.High, 12), null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient wallet balance.");
        repository.Rounds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsNewestRoundsForUserOnly()
    {
        var userId = Guid.NewGuid();
        var repository = new FakePlinkoRoundRepository();
        await repository.AddAsync(new PlinkoRound(userId, 10m, PlinkoRiskLevel.Low, 8, 4, 1.2m, "LRLRLRLR"));
        await repository.AddAsync(new PlinkoRound(Guid.NewGuid(), 50m, PlinkoRiskLevel.High, 8, 1, 3m, "LLLLLLLR"));
        var service = new PlinkoService(repository, new FakePlinkoWalletGateway(true), new Mock<IGamificationService>().Object);

        var history = await service.GetHistoryAsync(userId, 10);

        history.Should().ContainSingle();
        history.Single().Stake.Should().Be(10m);
    }

    private static PlinkoService CreateService()
    {
        return new PlinkoService(new FakePlinkoRoundRepository(), new FakePlinkoWalletGateway(true), new Mock<IGamificationService>().Object);
    }

    private sealed class FakePlinkoRoundRepository : IPlinkoRoundRepository
    {
        public List<PlinkoRound> Rounds { get; } = new();

        public Task AddAsync(PlinkoRound round)
        {
            Rounds.Add(round);
            return Task.CompletedTask;
        }

        public Task<PlinkoRound?> GetByIdAsync(Guid roundId)
        {
            return Task.FromResult(Rounds.SingleOrDefault(round => round.Id == roundId));
        }

        public Task UpdateAsync(PlinkoRound round)
        {
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<PlinkoRound>> GetRecentForUserAsync(Guid userId, int limit)
        {
            var result = Rounds
                .Where(round => round.UserId == userId)
                .OrderByDescending(round => round.CreatedAtUtc)
                .Take(limit)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<PlinkoRound>>(result);
        }
    }

    private sealed class FakePlinkoWalletGateway : IPlinkoWalletGateway
    {
        private readonly bool _stakeAccepted;

        public decimal ProcessedStake { get; private set; }
        public decimal ProcessedPayout { get; private set; }
        public int PayoutCallCount { get; private set; }

        public FakePlinkoWalletGateway(bool stakeAccepted)
        {
            _stakeAccepted = stakeAccepted;
        }

        public Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken)
        {
            ProcessedStake = amount;
            return Task.FromResult(_stakeAccepted);
        }

        public Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken)
        {
            ProcessedPayout = amount;
            PayoutCallCount += 1;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeResponsibleGamblingService : IResponsibleGamblingService
    {
        public decimal ValidatedStake { get; private set; }
        public List<RecordResponsibleGamblingActivityRequest> Activities { get; } = new();

        public Task<ResponsibleGamblingDashboardDto> GetDashboardAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponsibleGamblingLimitDto> SetLimitAsync(Guid userId, SetResponsibleGamblingLimitRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<SelfExclusionDto> StartSelfExclusionAsync(Guid userId, StartSelfExclusionRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ResponsibleGamblingValidationResultDto> ValidateStakeAsync(Guid userId, decimal amount)
        {
            ValidatedStake = amount;
            return Task.FromResult(new ResponsibleGamblingValidationResultDto(true, null, null, null, null, null));
        }

        public Task<ResponsibleGamblingValidationResultDto> ValidateDepositAsync(Guid userId, decimal amount)
        {
            throw new NotImplementedException();
        }

        public Task RecordActivityAsync(Guid userId, RecordResponsibleGamblingActivityRequest request)
        {
            Activities.Add(request);
            return Task.CompletedTask;
        }
    }
}
