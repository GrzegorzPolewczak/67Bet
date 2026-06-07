using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.ResponsibleGambling;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Application.Services;

public sealed class ResponsibleGamblingService : IResponsibleGamblingService
{
    private static readonly TimeSpan LimitIncreaseDelay = TimeSpan.FromHours(24);

    private readonly IResponsibleGamblingLimitRepository _limitRepository;
    private readonly ISelfExclusionRepository _selfExclusionRepository;
    private readonly IResponsibleGamblingActivityRepository _activityRepository;
    private readonly IResponsibleGamblingClock _clock;

    public ResponsibleGamblingService(
        IResponsibleGamblingLimitRepository limitRepository,
        ISelfExclusionRepository selfExclusionRepository,
        IResponsibleGamblingActivityRepository activityRepository,
        IResponsibleGamblingClock clock)
    {
        _limitRepository = limitRepository;
        _selfExclusionRepository = selfExclusionRepository;
        _activityRepository = activityRepository;
        _clock = clock;
    }

    public async Task<ResponsibleGamblingDashboardDto> GetDashboardAsync(Guid userId)
    {
        var now = _clock.UtcNow;
        var limits = await GetLimitsWithAppliedPendingChangesAsync(userId, now);
        var usage = await BuildUsageAsync(userId, limits, now);
        var activeSelfExclusion = await _selfExclusionRepository.GetActiveForUserAsync(userId, now);
        var history = await _selfExclusionRepository.GetRecentForUserAsync(userId, 10);

        return new ResponsibleGamblingDashboardDto(
            limits.Select(ToDto).ToArray(),
            usage,
            activeSelfExclusion == null ? null : ToDto(activeSelfExclusion, now),
            history.Select(item => ToDto(item, now)).ToArray());
    }

    public async Task<ResponsibleGamblingLimitDto> SetLimitAsync(Guid userId, SetResponsibleGamblingLimitRequest request)
    {
        if (request.Amount <= 0) throw new InvalidOperationException("Limit amount must be greater than zero.");

        var now = _clock.UtcNow;
        var existing = await _limitRepository.GetByUserAndTypeAsync(userId, request.Type);
        if (existing == null)
        {
            var limit = new ResponsibleGamblingLimit(userId, request.Type, request.Amount, now);
            await _limitRepository.AddAsync(limit);
            return ToDto(limit);
        }

        existing.ApplyPendingChange(now);
        existing.RequestAmountChange(request.Amount, now, LimitIncreaseDelay);
        await _limitRepository.UpdateAsync(existing);
        return ToDto(existing);
    }

    public async Task<SelfExclusionDto> StartSelfExclusionAsync(Guid userId, StartSelfExclusionRequest request)
    {
        if (request.DurationHours < 24) throw new InvalidOperationException("Self-exclusion must last at least 24 hours.");
        if (request.DurationHours > 24 * 365) throw new InvalidOperationException("Self-exclusion cannot exceed 365 days.");

        var now = _clock.UtcNow;
        var active = await _selfExclusionRepository.GetActiveForUserAsync(userId, now);
        if (active != null) throw new InvalidOperationException("Self-exclusion is already active.");

        var exclusion = new SelfExclusion(userId, now, now.AddHours(request.DurationHours), request.Reason);
        await _selfExclusionRepository.AddAsync(exclusion);
        return ToDto(exclusion, now);
    }

    public async Task<ResponsibleGamblingValidationResultDto> ValidateStakeAsync(Guid userId, decimal amount)
    {
        if (amount <= 0) return Denied("INVALID_AMOUNT", "Amount must be greater than zero.");

        var now = _clock.UtcNow;
        var selfExclusion = await _selfExclusionRepository.GetActiveForUserAsync(userId, now);
        if (selfExclusion != null)
        {
            return Denied("SELF_EXCLUSION_ACTIVE", "Self-exclusion is active.", blockedUntilUtc: selfExclusion.EndsAtUtc);
        }

        var limits = await GetLimitsWithAppliedPendingChangesAsync(userId, now);
        var usage = await BuildUsageAsync(userId, limits, now);

        var singleStake = limits.FirstOrDefault(limit => limit.Type == ResponsibleLimitType.SingleStake);
        if (singleStake != null && amount > singleStake.Amount)
        {
            return Denied("SINGLE_STAKE_LIMIT", "Stake exceeds the single stake limit.", singleStake.Amount, 0);
        }

        var dailyStake = limits.FirstOrDefault(limit => limit.Type == ResponsibleLimitType.DailyStake);
        if (dailyStake != null && usage.DailyStakeUsed + amount > dailyStake.Amount)
        {
            return Denied("DAILY_STAKE_LIMIT", "Stake exceeds the daily stake limit.", dailyStake.Amount, usage.DailyStakeUsed);
        }

        var weeklyLoss = limits.FirstOrDefault(limit => limit.Type == ResponsibleLimitType.WeeklyLoss);
        if (weeklyLoss != null && usage.WeeklyNetLoss + amount > weeklyLoss.Amount)
        {
            return Denied("WEEKLY_LOSS_LIMIT", "Stake would exceed the weekly loss limit.", weeklyLoss.Amount, usage.WeeklyNetLoss);
        }

        return Allowed();
    }

    public async Task<ResponsibleGamblingValidationResultDto> ValidateDepositAsync(Guid userId, decimal amount)
    {
        if (amount <= 0) return Denied("INVALID_AMOUNT", "Amount must be greater than zero.");

        var now = _clock.UtcNow;
        var selfExclusion = await _selfExclusionRepository.GetActiveForUserAsync(userId, now);
        if (selfExclusion != null)
        {
            return Denied("SELF_EXCLUSION_ACTIVE", "Self-exclusion is active.", blockedUntilUtc: selfExclusion.EndsAtUtc);
        }

        var limits = await GetLimitsWithAppliedPendingChangesAsync(userId, now);
        var usage = await BuildUsageAsync(userId, limits, now);
        var dailyDeposit = limits.FirstOrDefault(limit => limit.Type == ResponsibleLimitType.DailyDeposit);

        if (dailyDeposit != null && usage.DailyDepositUsed + amount > dailyDeposit.Amount)
        {
            return Denied("DAILY_DEPOSIT_LIMIT", "Deposit exceeds the daily deposit limit.", dailyDeposit.Amount, usage.DailyDepositUsed);
        }

        return Allowed();
    }

    public async Task RecordActivityAsync(Guid userId, RecordResponsibleGamblingActivityRequest request)
    {
        if (request.Amount <= 0) throw new InvalidOperationException("Activity amount must be greater than zero.");

        var activity = new ResponsibleGamblingActivity(userId, request.Type, request.Amount, _clock.UtcNow);
        await _activityRepository.AddAsync(activity);
    }

    private async Task<IReadOnlyCollection<ResponsibleGamblingLimit>> GetLimitsWithAppliedPendingChangesAsync(Guid userId, DateTime now)
    {
        var limits = await _limitRepository.GetByUserIdAsync(userId);
        foreach (var limit in limits)
        {
            if (limit.ApplyPendingChange(now))
            {
                await _limitRepository.UpdateAsync(limit);
            }
        }

        return limits;
    }

    private async Task<ResponsibleGamblingUsageDto> BuildUsageAsync(
        Guid userId,
        IReadOnlyCollection<ResponsibleGamblingLimit> limits,
        DateTime now)
    {
        var weekStart = now.AddDays(-7);
        var activities = await _activityRepository.GetForUserSinceAsync(userId, weekStart);
        var dayStart = now.Date;

        var dailyStake = activities
            .Where(activity => activity.Type == ResponsibleGamblingActivityType.Stake && activity.OccurredAtUtc >= dayStart)
            .Sum(activity => activity.Amount);

        var dailyDeposit = activities
            .Where(activity => activity.Type == ResponsibleGamblingActivityType.Deposit && activity.OccurredAtUtc >= dayStart)
            .Sum(activity => activity.Amount);

        var weeklyStake = activities
            .Where(activity => activity.Type == ResponsibleGamblingActivityType.Stake)
            .Sum(activity => activity.Amount);

        var weeklyPayout = activities
            .Where(activity => activity.Type == ResponsibleGamblingActivityType.Payout)
            .Sum(activity => activity.Amount);

        var weeklyNetLoss = Math.Max(0, weeklyStake - weeklyPayout);

        return new ResponsibleGamblingUsageDto(
            dailyStake,
            dailyDeposit,
            weeklyNetLoss,
            Remaining(limits, ResponsibleLimitType.DailyStake, dailyStake),
            Remaining(limits, ResponsibleLimitType.DailyDeposit, dailyDeposit),
            Remaining(limits, ResponsibleLimitType.WeeklyLoss, weeklyNetLoss));
    }

    private static decimal? Remaining(IReadOnlyCollection<ResponsibleGamblingLimit> limits, ResponsibleLimitType type, decimal used)
    {
        var limit = limits.FirstOrDefault(item => item.Type == type);
        return limit == null ? null : Math.Max(0, limit.Amount - used);
    }

    private static ResponsibleGamblingLimitDto ToDto(ResponsibleGamblingLimit limit)
    {
        return new ResponsibleGamblingLimitDto(
            limit.Id,
            limit.Type,
            limit.Period,
            limit.Amount,
            limit.PendingAmount,
            limit.PendingActivationUtc,
            limit.UpdatedAtUtc);
    }

    private static SelfExclusionDto ToDto(SelfExclusion exclusion, DateTime now)
    {
        return new SelfExclusionDto(
            exclusion.Id,
            exclusion.StartsAtUtc,
            exclusion.EndsAtUtc,
            exclusion.Reason,
            exclusion.IsActiveAt(now));
    }

    private static ResponsibleGamblingValidationResultDto Allowed()
    {
        return new ResponsibleGamblingValidationResultDto(true, null, null, null, null, null);
    }

    private static ResponsibleGamblingValidationResultDto Denied(
        string reasonCode,
        string message,
        decimal? limitAmount = null,
        decimal? currentUsage = null,
        DateTime? blockedUntilUtc = null)
    {
        return new ResponsibleGamblingValidationResultDto(false, reasonCode, message, limitAmount, currentUsage, blockedUntilUtc);
    }
}
