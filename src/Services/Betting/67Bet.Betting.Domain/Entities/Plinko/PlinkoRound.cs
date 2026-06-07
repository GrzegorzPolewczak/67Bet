namespace _67Bet.Betting.Domain.Entities.Plinko;

public enum PlinkoRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3
}

public sealed class PlinkoRound
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public decimal Stake { get; private set; }
    public PlinkoRiskLevel RiskLevel { get; private set; }
    public int Rows { get; private set; }
    public int LandingSlot { get; private set; }
    public decimal Multiplier { get; private set; }
    public decimal Payout { get; private set; }
    public bool IsPayoutSettled { get; private set; }
    public string Path { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private PlinkoRound()
    {
    }

    public PlinkoRound(Guid userId, decimal stake, PlinkoRiskLevel riskLevel, int rows, int landingSlot, decimal multiplier, string path)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (stake <= 0) throw new ArgumentOutOfRangeException(nameof(stake), "Stake must be greater than zero.");
        if (rows < 8 || rows > 16) throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be between 8 and 16.");
        if (landingSlot < 0 || landingSlot > rows) throw new ArgumentOutOfRangeException(nameof(landingSlot), "Landing slot must fit rows.");
        if (multiplier < 0) throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier cannot be negative.");
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path is required.", nameof(path));

        UserId = userId;
        Stake = Math.Round(stake, 2);
        RiskLevel = riskLevel;
        Rows = rows;
        LandingSlot = landingSlot;
        Multiplier = multiplier;
        Path = path;
        Payout = Math.Round(Stake * Multiplier, 2);
    }

    public void MarkPayoutSettled()
    {
        IsPayoutSettled = true;
    }
}
