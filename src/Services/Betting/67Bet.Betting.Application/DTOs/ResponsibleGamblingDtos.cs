using _67Bet.Betting.Domain.Enums;

namespace _67Bet.Betting.Application.DTOs;

public record ResponsibleGamblingLimitDto(
    Guid Id,
    ResponsibleLimitType Type,
    ResponsibleLimitPeriod Period,
    decimal Amount,
    decimal? PendingAmount,
    DateTime? PendingActivationUtc,
    DateTime UpdatedAtUtc);

public record ResponsibleGamblingUsageDto(
    decimal DailyStakeUsed,
    decimal DailyDepositUsed,
    decimal WeeklyNetLoss,
    decimal? DailyStakeRemaining,
    decimal? DailyDepositRemaining,
    decimal? WeeklyLossRemaining);

public record SelfExclusionDto(
    Guid Id,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Reason,
    bool IsActive);

public record ResponsibleGamblingDashboardDto(
    IReadOnlyCollection<ResponsibleGamblingLimitDto> Limits,
    ResponsibleGamblingUsageDto Usage,
    SelfExclusionDto? ActiveSelfExclusion,
    IReadOnlyCollection<SelfExclusionDto> SelfExclusionHistory);

public record SetResponsibleGamblingLimitRequest(ResponsibleLimitType Type, decimal Amount);

public record StartSelfExclusionRequest(int DurationHours, string? Reason);

public record ResponsibleGamblingValidationRequest(decimal Amount);

public record ResponsibleGamblingValidationResultDto(
    bool IsAllowed,
    string? ReasonCode,
    string? Message,
    decimal? LimitAmount,
    decimal? CurrentUsage,
    DateTime? BlockedUntilUtc);

public record RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType Type, decimal Amount);
