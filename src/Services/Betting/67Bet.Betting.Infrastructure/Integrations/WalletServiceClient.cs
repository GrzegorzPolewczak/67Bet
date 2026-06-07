using System.Net.Http.Json;
using _67Bet.Wallet.Application.Interfaces;
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

        var baseUrl = configuration["WalletService:BaseUrl"] ?? "http://localhost:5200/api/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"wallet/balance?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
                return result?.Balance ?? 0;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Wallet balance request failed. BaseAddress={BaseAddress}, UserId={UserId}, Status={StatusCode}, Body={Body}",
                _httpClient.BaseAddress,
                userId,
                response.StatusCode,
                body);
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
            var response = await _httpClient.GetAsync($"wallet/balance?userId={userId}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
                return result?.FreebetBalance ?? 0;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Wallet freebet balance request failed. BaseAddress={BaseAddress}, UserId={UserId}, Status={StatusCode}, Body={Body}",
                _httpClient.BaseAddress,
                userId,
                response.StatusCode,
                body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get freebet balance for user {UserId}", userId);
        }

        return 0;
    }

    public async Task DepositAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("wallet/deposit", new { UserId = userId, Amount = amount });
    }

    public async Task DepositFreebetAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("wallet/deposit-freebet", new { UserId = userId, Amount = amount });
    }

    public async Task WithdrawAsync(Guid userId, decimal amount)
    {
        await _httpClient.PostAsJsonAsync("wallet/withdraw", new { UserId = userId, Amount = amount });
    }

    public async Task<bool> ProcessStakeAsync(Guid userId, decimal amount)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "wallet/process-stake",
                new { UserId = userId, Amount = amount });

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Wallet accepted stake. BaseAddress={BaseAddress}, UserId={UserId}, Amount={Amount}",
                    _httpClient.BaseAddress,
                    userId,
                    amount);

                return true;
            }

            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning(
                "Wallet rejected stake. BaseAddress={BaseAddress}, UserId={UserId}, Amount={Amount}, Status={StatusCode}, Body={Body}",
                _httpClient.BaseAddress,
                userId,
                amount,
                response.StatusCode,
                body);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process stake. BaseAddress={BaseAddress}, UserId={UserId}, Amount={Amount}",
                _httpClient.BaseAddress,
                userId,
                amount);

            throw;
        }
    }

    public async Task ProcessPayoutAsync(Guid userId, decimal amount)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "wallet/process-payout",
                new { UserId = userId, Amount = amount });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Wallet payout failed. BaseAddress={BaseAddress}, UserId={UserId}, Amount={Amount}, Status={StatusCode}, Body={Body}",
                    _httpClient.BaseAddress,
                    userId,
                    amount,
                    response.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payout for user {UserId}", userId);
            throw;
        }
    }

    public async Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetWalletByUserIdAsync(Guid userId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<_67Bet.Wallet.Domain.Entities.Wallet>($"wallet/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get wallet for user {UserId}", userId);
            return null;
        }
    }

    private class BalanceResponse
    {
        public decimal Balance { get; set; }
        public decimal FreebetBalance { get; set; }
    }
}
