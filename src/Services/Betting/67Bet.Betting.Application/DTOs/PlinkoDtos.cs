using _67Bet.Betting.Domain.Entities.Plinko;

namespace _67Bet.Betting.Application.DTOs;

public sealed record PlinkoPlayRequest(decimal Stake, PlinkoRiskLevel RiskLevel, int Rows = 12, decimal BoardFee = 0);

public sealed record PlinkoRoundDto(
    Guid Id,
    decimal Stake,
    PlinkoRiskLevel RiskLevel,
    int Rows,
    int LandingSlot,
    decimal Multiplier,
    decimal Payout,
    string Path,
    DateTime CreatedAtUtc);

public sealed record PlinkoBoardDto(int Rows, PlinkoRiskLevel RiskLevel, IReadOnlyList<decimal> Multipliers);
