using _67Bet.Betting.Application.DTOs;

namespace _67Bet.Betting.Application.Interfaces;

public interface IRouletteService
{
    Task<RouletteRoundDto> PlayAsync(Guid userId, RoulettePlayRequest request, string? bearerToken);
    Task<RouletteRoundDto> SettleRoundAsync(Guid userId, Guid roundId, string? bearerToken);
    Task<IReadOnlyCollection<RouletteRoundDto>> GetHistoryAsync(Guid userId, int limit);
}
