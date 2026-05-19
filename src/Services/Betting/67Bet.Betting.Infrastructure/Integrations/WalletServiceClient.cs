using System.Net.Http.Json;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Infrastructure.Integrations;

public class WalletServiceClient : IWalletService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WalletServiceClient> _logger;

    public WalletServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<WalletServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        var baseUrl = configuration["WalletService:BaseUrl"] ?? "http://localhost:5400/api/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        try
        {
            // Note: In a real scenario, we might need to pass the userId in a header or as a query param
            // if the service is called on behalf of a user.
            var response = await _httpClient.GetAsync($"Wallet/balance?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
                return result?.Balance ?? 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get balance for user {UserId}", userId);
        }
        return 0;
    }

    public async Task<decimal> GetFreebetBalanceAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Wallet/balance?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
                return result?.FreebetBalance ?? 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get freebet balance for user {UserId}", userId);
        }
        return 0;
    }

    public async Task DepositAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("Wallet/deposit", new { UserId = userId, Amount = amount });
    }

    public async Task DepositFreebetAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("Wallet/deposit-freebet", new { UserId = userId, Amount = amount });
    }

    public async Task WithdrawAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("Wallet/withdraw", new { UserId = userId, Amount = amount });
    }

    public async Task<bool> ProcessStakeAsync(Guid userId, decimal amount)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Wallet/process-stake", new { UserId = userId, Amount = amount });
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process stake for user {UserId}", userId);
            return false;
        }
    }

    public async Task ProcessPayoutAsync(Guid userId, decimal amount)
    {
        try
        {
            await _httpClient.PostAsJsonAsync("Wallet/process-payout", new { UserId = userId, Amount = amount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payout for user {UserId}", userId);
        }
    }

    public async Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetWalletByUserIdAsync(Guid userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<_67Bet.Wallet.Domain.Entities.Wallet>($"Wallet/{userId}");
        }
        catch
        {
            return null;
        }
    }

    private class BalanceResponse
    {
        public decimal Balance { get; set; }
        public decimal FreebetBalance { get; set; }
    }
}
