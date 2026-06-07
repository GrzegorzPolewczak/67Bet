using _67Bet.CustomBet.Application.DTOs;
using _67Bet.CustomBet.Domain.Entities;

namespace _67Bet.CustomBet.Application.Mappings;

public static class CustomBetMappingExtensions
{
    public static CustomBetRequestDto ToDto(this CustomBetRequest request)
    {
        return new CustomBetRequestDto(
            request.Id,
            request.UserId,
            request.Description,
            request.Status.ToString(),
            request.AiSuggestedOdds,
            request.AiAnalysisNote,
            request.AiRiskLevel,
            request.AiCategory,
            request.AdminFinalOdds,
            request.AdminNote,
            request.CreatedAt
        );
    }
}
