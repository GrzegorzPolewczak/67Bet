namespace _67Bet.Betting.Application.Interfaces;

public interface IRouletteWalletGateway
{
    Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken);
    Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken);
}
