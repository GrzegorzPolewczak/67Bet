namespace _67Bet.Betting.Domain.Entities.Roulette;

public sealed class RouletteRound
{
    private readonly List<RouletteBet> _bets = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public IReadOnlyCollection<RouletteBet> Bets => _bets.AsReadOnly();
    public int SpinResult { get; private set; }
    public decimal TotalStake { get; private set; }
    public decimal TotalPayout { get; private set; }
    public bool IsPayoutSettled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private RouletteRound() { }

    public RouletteRound(Guid userId, IReadOnlyList<RouletteBet> bets, int spinResult)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User id is required.", nameof(userId));
        if (bets == null || bets.Count == 0) throw new ArgumentException("At least one bet is required.", nameof(bets));
        if (bets.Count > 10) throw new ArgumentException("Maximum 10 bets per round.", nameof(bets));
        if (spinResult is < 0 or > 36) throw new ArgumentOutOfRangeException(nameof(spinResult), "Spin result must be between 0 and 36.");

        UserId = userId;
        SpinResult = spinResult;
        _bets.AddRange(bets);
        TotalStake = Math.Round(_bets.Sum(b => b.Stake), 2);

        foreach (var bet in _bets)
            bet.Settle(spinResult);

        TotalPayout = Math.Round(_bets.Sum(b => b.Payout), 2);
    }

    public void MarkPayoutSettled()
    {
        IsPayoutSettled = true;
    }
}
