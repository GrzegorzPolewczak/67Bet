namespace _67Bet.Betting.Application.Interfaces;

public interface IPlinkoWalletGateway
{
    Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken);
    Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken);
}
