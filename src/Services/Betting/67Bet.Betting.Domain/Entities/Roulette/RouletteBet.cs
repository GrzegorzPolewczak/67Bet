namespace _67Bet.Betting.Domain.Entities.Roulette;

public enum RouletteBetType
{
    StraightUp = 0,
    Red = 1,
    Black = 2,
    Even = 3,
    Odd = 4,
    Low = 5,
    High = 6,
    DozenFirst = 7,
    DozenSecond = 8,
    DozenThird = 9,
    ColumnFirst = 10,
    ColumnSecond = 11,
    ColumnThird = 12
}

public sealed class RouletteBet
{
    private static readonly HashSet<int> RedNumbers = new() { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

    public Guid Id { get; private set; } = Guid.NewGuid();
    public RouletteBetType BetType { get; private set; }
    public int? ChosenNumber { get; private set; }
    public decimal Stake { get; private set; }
    public bool IsWon { get; private set; }
    public decimal Payout { get; private set; }

    private RouletteBet() { }

    public RouletteBet(RouletteBetType betType, int? chosenNumber, decimal stake)
    {
        if (stake <= 0) throw new ArgumentOutOfRangeException(nameof(stake), "Stake must be greater than zero.");
        if (betType == RouletteBetType.StraightUp && (chosenNumber is null or < 0 or > 36))
            throw new ArgumentException("Straight up bet requires a number between 0 and 36.", nameof(chosenNumber));

        BetType = betType;
        ChosenNumber = betType == RouletteBetType.StraightUp ? chosenNumber : null;
        Stake = Math.Round(stake, 2);
    }

    internal void Settle(int spinResult)
    {
        IsWon = IsWinning(spinResult);
        Payout = IsWon ? Math.Round(Stake * GetMultiplier(), 2) : 0m;
    }

    private bool IsWinning(int spinResult) => BetType switch
    {
        RouletteBetType.StraightUp => ChosenNumber == spinResult,
        RouletteBetType.Red => RedNumbers.Contains(spinResult),
        RouletteBetType.Black => spinResult != 0 && !RedNumbers.Contains(spinResult),
        RouletteBetType.Even => spinResult != 0 && spinResult % 2 == 0,
        RouletteBetType.Odd => spinResult % 2 == 1,
        RouletteBetType.Low => spinResult is >= 1 and <= 18,
        RouletteBetType.High => spinResult is >= 19 and <= 36,
        RouletteBetType.DozenFirst => spinResult is >= 1 and <= 12,
        RouletteBetType.DozenSecond => spinResult is >= 13 and <= 24,
        RouletteBetType.DozenThird => spinResult is >= 25 and <= 36,
        RouletteBetType.ColumnFirst => spinResult != 0 && spinResult % 3 == 1,
        RouletteBetType.ColumnSecond => spinResult != 0 && spinResult % 3 == 2,
        RouletteBetType.ColumnThird => spinResult != 0 && spinResult % 3 == 0,
        _ => false
    };

    private decimal GetMultiplier() => BetType switch
    {
        RouletteBetType.StraightUp => 36m,
        RouletteBetType.Red or RouletteBetType.Black or
        RouletteBetType.Even or RouletteBetType.Odd or
        RouletteBetType.Low or RouletteBetType.High => 2m,
        _ => 3m
    };
}
