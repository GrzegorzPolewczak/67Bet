using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Domain.Entities.Plinko;

namespace _67Bet.Betting.Application.Interfaces;

public interface IPlinkoService
{
    PlinkoBoardDto GetBoard(PlinkoRiskLevel riskLevel, int rows);
    Task<PlinkoRoundDto> PlayAsync(Guid userId, PlinkoPlayRequest request, string? bearerToken);
    Task<PlinkoRoundDto> SettleRoundAsync(Guid userId, Guid roundId, string? bearerToken);
    Task<IReadOnlyCollection<PlinkoRoundDto>> GetHistoryAsync(Guid userId, int limit);
}
