using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.ResponsibleGambling;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using FluentAssertions;

namespace _67Bet.UnitTests.Services;

public class ResponsibleGamblingServiceTests
{
    [Fact]
    public async Task SetLimitAsync_WhenLimitIncreaseRequested_CreatesPendingChange()
    {
        var userId = Guid.NewGuid();
        var clock = new FakeClock(new DateTime(2026, 6, 7, 10, 0, 0, DateTimeKind.Utc));
        var limitRepository = new FakeLimitRepository();
        var service = CreateService(limitRepository, clock: clock);

        await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.DailyStake, 100m));
        var changed = await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.DailyStake, 200m));

        changed.Amount.Should().Be(100m);
        changed.PendingAmount.Should().Be(200m);
        changed.PendingActivationUtc.Should().Be(clock.UtcNow.AddHours(24));
        limitRepository.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task SetLimitAsync_WhenLimitDecreaseRequested_AppliesImmediately()
    {
        var userId = Guid.NewGuid();
        var service = CreateService();

        await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.SingleStake, 100m));
        var changed = await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.SingleStake, 50m));

        changed.Amount.Should().Be(50m);
        changed.PendingAmount.Should().BeNull();
    }

    [Fact]
    public async Task ValidateStakeAsync_WhenSingleStakeLimitExceeded_DeniesRequest()
    {
        var userId = Guid.NewGuid();
        var service = CreateService();
        await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.SingleStake, 25m));

        var result = await service.ValidateStakeAsync(userId, 30m);

        result.IsAllowed.Should().BeFalse();
        result.ReasonCode.Should().Be("SINGLE_STAKE_LIMIT");
        result.LimitAmount.Should().Be(25m);
    }

    [Fact]
    public async Task ValidateStakeAsync_WhenDailyStakeUsageWouldExceedLimit_DeniesRequest()
    {
        var userId = Guid.NewGuid();
        var activityRepository = new FakeActivityRepository();
        var service = CreateService(activityRepository: activityRepository);
        await service.SetLimitAsync(userId, new SetResponsibleGamblingLimitRequest(ResponsibleLimitType.DailyStake, 100m));
        await activityRepository.AddAsync(new ResponsibleGamblingActivity(userId, ResponsibleGamblingActivityType.Stake, 80m, DateTime.UtcNow));

        var result = await service.ValidateStakeAsync(userId, 25m);

        result.IsAllowed.Should().BeFalse();
        result.ReasonCode.Should().Be("DAILY_STAKE_LIMIT");
        result.CurrentUsage.Should().Be(80m);
    }

    [Fact]
    public async Task ValidateStakeAsync_WhenSelfExclusionIsActive_DeniesRequest()
    {
        var userId = Guid.NewGuid();
        var clock = new FakeClock(new DateTime(2026, 6, 7, 10, 0, 0, DateTimeKind.Utc));
        var service = CreateService(clock: clock);
        await service.StartSelfExclusionAsync(userId, new StartSelfExclusionRequest(24, "Cooling off"));

        var result = await service.ValidateStakeAsync(userId, 10m);

        result.IsAllowed.Should().BeFalse();
        result.ReasonCode.Should().Be("SELF_EXCLUSION_ACTIVE");
        result.BlockedUntilUtc.Should().Be(clock.UtcNow.AddHours(24));
    }

    private static ResponsibleGamblingService CreateService(
        FakeLimitRepository? limitRepository = null,
        FakeSelfExclusionRepository? selfExclusionRepository = null,
        FakeActivityRepository? activityRepository = null,
        IResponsibleGamblingClock? clock = null)
    {
        return new ResponsibleGamblingService(
            limitRepository ?? new FakeLimitRepository(),
            selfExclusionRepository ?? new FakeSelfExclusionRepository(),
            activityRepository ?? new FakeActivityRepository(),
            clock ?? new FakeClock(DateTime.UtcNow));
    }

    private sealed class FakeClock : IResponsibleGamblingClock
    {
        public FakeClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; set; }
    }

    private sealed class FakeLimitRepository : IResponsibleGamblingLimitRepository
    {
        public List<ResponsibleGamblingLimit> Items { get; } = new();

        public Task<ResponsibleGamblingLimit?> GetByUserAndTypeAsync(Guid userId, ResponsibleLimitType type)
        {
            return Task.FromResult(Items.FirstOrDefault(limit => limit.UserId == userId && limit.Type == type));
        }

        public Task<IReadOnlyCollection<ResponsibleGamblingLimit>> GetByUserIdAsync(Guid userId)
        {
            return Task.FromResult<IReadOnlyCollection<ResponsibleGamblingLimit>>(
                Items.Where(limit => limit.UserId == userId).ToArray());
        }

        public Task<ResponsibleGamblingLimit?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Items.FirstOrDefault(limit => limit.Id == id));
        }

        public Task<IEnumerable<ResponsibleGamblingLimit>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<ResponsibleGamblingLimit>>(Items);
        }

        public Task AddAsync(ResponsibleGamblingLimit entity)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ResponsibleGamblingLimit entity)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ResponsibleGamblingLimit entity)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSelfExclusionRepository : ISelfExclusionRepository
    {
        private readonly List<SelfExclusion> _items = new();

        public Task<SelfExclusion?> GetActiveForUserAsync(Guid userId, DateTime nowUtc)
        {
            return Task.FromResult(_items.FirstOrDefault(item => item.UserId == userId && item.IsActiveAt(nowUtc)));
        }

        public Task<IReadOnlyCollection<SelfExclusion>> GetRecentForUserAsync(Guid userId, int limit)
        {
            return Task.FromResult<IReadOnlyCollection<SelfExclusion>>(
                _items.Where(item => item.UserId == userId).Take(limit).ToArray());
        }

        public Task<SelfExclusion?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
        }

        public Task<IEnumerable<SelfExclusion>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<SelfExclusion>>(_items);
        }

        public Task AddAsync(SelfExclusion entity)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SelfExclusion entity)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(SelfExclusion entity)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeActivityRepository : IResponsibleGamblingActivityRepository
    {
        private readonly List<ResponsibleGamblingActivity> _items = new();

        public Task<IReadOnlyCollection<ResponsibleGamblingActivity>> GetForUserSinceAsync(Guid userId, DateTime sinceUtc)
        {
            return Task.FromResult<IReadOnlyCollection<ResponsibleGamblingActivity>>(
                _items.Where(item => item.UserId == userId && item.OccurredAtUtc >= sinceUtc).ToArray());
        }

        public Task<ResponsibleGamblingActivity?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_items.FirstOrDefault(item => item.Id == id));
        }

        public Task<IEnumerable<ResponsibleGamblingActivity>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<ResponsibleGamblingActivity>>(_items);
        }

        public Task AddAsync(ResponsibleGamblingActivity entity)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ResponsibleGamblingActivity entity)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ResponsibleGamblingActivity entity)
        {
            _items.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
