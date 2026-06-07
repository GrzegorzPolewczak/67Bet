using _67Bet.Betting.Domain.Enums;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.ResponsibleGambling;

public sealed class ResponsibleGamblingActivity : BaseEntity
{
    public Guid UserId { get; private set; }
    public ResponsibleGamblingActivityType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private ResponsibleGamblingActivity()
    {
    }

    public ResponsibleGamblingActivity(Guid userId, ResponsibleGamblingActivityType type, decimal amount, DateTime occurredAtUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Activity amount must be greater than zero.");

        UserId = userId;
        Type = type;
        Amount = Math.Round(amount, 2);
        OccurredAtUtc = occurredAtUtc;
    }
}
