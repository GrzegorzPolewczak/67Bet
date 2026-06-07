using _67Bet.Betting.Domain.Entities.Roulette;

namespace _67Bet.Betting.Application.DTOs;

public sealed record RouletteBetRequest(RouletteBetType BetType, int? ChosenNumber, decimal Stake);

public sealed record RoulettePlayRequest(IReadOnlyList<RouletteBetRequest> Bets);

public sealed record RouletteBetDto(
    Guid Id,
    RouletteBetType BetType,
    int? ChosenNumber,
    decimal Stake,
    bool IsWon,
    decimal Payout);

public sealed record RouletteRoundDto(
    Guid Id,
    int SpinResult,
    decimal TotalStake,
    decimal TotalPayout,
    bool IsPayoutSettled,
    DateTime CreatedAtUtc,
    IReadOnlyList<RouletteBetDto> Bets);
