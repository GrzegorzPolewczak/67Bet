using System;
using System.Collections.Generic;

namespace _67Bet.Betting.Application.DTOs;

public record OutcomeDto(Guid Id, string Name, decimal CurrentPrice);

public record MarketDto(Guid Id, string Name, List<OutcomeDto> Outcomes);

public record EventDto(Guid Id, string Name, DateTime StartTime, string Status, List<MarketDto> Markets);

public record BetDto(Guid OutcomeId, decimal FixedPrice, string Status);

public record TicketDto(Guid Id, decimal Stake, decimal TotalOdds, decimal PotentialWinning, string Status, List<BetDto> Bets);

public record PlaceTicketRequest(decimal Stake, List<Guid> OutcomeIds);

public record SettleEventRequest(List<Guid> WinningOutcomeIds);
