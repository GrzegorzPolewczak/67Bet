using System;

namespace _67Bet.CustomBet.Application.DTOs;

public record CustomBetRequestDto(
    Guid Id,
    string Description,
    string Status,
    decimal AiSuggestedOdds,
    decimal? AdminFinalOdds,
    string? AdminNote
);

public record CreateCustomBetRequest(string Description);

public record AcceptCustomBetRequest(decimal FinalOdds, string? AdminNote);

public record RejectCustomBetRequest(string Reason);
