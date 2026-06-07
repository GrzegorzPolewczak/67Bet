using _67Bet.Betting.Domain.Enums;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.ResponsibleGambling;

public sealed class ResponsibleGamblingLimit : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public ResponsibleLimitType Type { get; private set; }
    public ResponsibleLimitPeriod Period { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? PendingAmount { get; private set; }
    public DateTime? PendingActivationUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private ResponsibleGamblingLimit()
    {
    }

    public ResponsibleGamblingLimit(Guid userId, ResponsibleLimitType type, decimal amount, DateTime nowUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Limit amount must be greater than zero.");

        UserId = userId;
        Type = type;
        Period = ResolvePeriod(type);
        Amount = Math.Round(amount, 2);
        UpdatedAtUtc = nowUtc;
    }

    public bool RequestAmountChange(decimal amount, DateTime nowUtc, TimeSpan increaseDelay)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Limit amount must be greater than zero.");

        var roundedAmount = Math.Round(amount, 2);
        if (roundedAmount <= Amount)
        {
            Amount = roundedAmount;
            PendingAmount = null;
            PendingActivationUtc = null;
            UpdatedAtUtc = nowUtc;
            return true;
        }

        PendingAmount = roundedAmount;
        PendingActivationUtc = nowUtc.Add(increaseDelay);
        UpdatedAtUtc = nowUtc;
        return false;
    }

    public bool ApplyPendingChange(DateTime nowUtc)
    {
        if (PendingAmount == null || PendingActivationUtc == null || PendingActivationUtc > nowUtc)
        {
            return false;
        }

        Amount = PendingAmount.Value;
        PendingAmount = null;
        PendingActivationUtc = null;
        UpdatedAtUtc = nowUtc;
        return true;
    }

    private static ResponsibleLimitPeriod ResolvePeriod(ResponsibleLimitType type)
    {
        return type switch
        {
            ResponsibleLimitType.SingleStake => ResponsibleLimitPeriod.None,
            ResponsibleLimitType.DailyStake => ResponsibleLimitPeriod.Daily,
            ResponsibleLimitType.DailyDeposit => ResponsibleLimitPeriod.Daily,
            ResponsibleLimitType.WeeklyLoss => ResponsibleLimitPeriod.Weekly,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unsupported limit type.")
        };
    }
}
