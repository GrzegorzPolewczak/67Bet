using System;

namespace _67Bet.Odds.Application.DTOs;

public record CalculateOddsRequest(decimal Probability);

public record OddsResultDto(decimal Odds, decimal Probability);

public record LiveProbabilityRequest(Guid EventId, string ContextJson);
