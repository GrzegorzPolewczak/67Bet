using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.ResponsibleGambling;

public sealed class SelfExclusion : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    private SelfExclusion()
    {
    }

    public SelfExclusion(Guid userId, DateTime startsAtUtc, DateTime endsAtUtc, string? reason)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (endsAtUtc <= startsAtUtc) throw new ArgumentException("Self-exclusion end date must be later than start date.");

        UserId = userId;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Reason = string.IsNullOrWhiteSpace(reason) ? "Responsible gambling cooling-off" : reason.Trim();
    }

    public bool IsActiveAt(DateTime nowUtc)
    {
        return StartsAtUtc <= nowUtc && EndsAtUtc > nowUtc;
    }
}
