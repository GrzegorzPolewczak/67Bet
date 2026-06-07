using System;

namespace _67Bet.CustomBet.Application.DTOs;

public record CustomBetRequestDto(
    Guid Id,
    Guid UserId,
    string Description,
    string Status,
    decimal AiSuggestedOdds,
    string? AiAnalysisNote,
    string? AiRiskLevel,
    string? AiCategory,
    decimal? AdminFinalOdds,
    string? AdminNote,
    DateTime CreatedAt
);

public class CreateCustomBetRequest
{
    public string Description { get; set; } = string.Empty;
}

public record AcceptCustomBetRequest(decimal FinalOdds, string? AdminNote);

public record RejectCustomBetRequest(string Reason);
