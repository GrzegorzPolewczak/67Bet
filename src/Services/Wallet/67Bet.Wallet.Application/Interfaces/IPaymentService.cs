using System.Threading.Tasks;
using _67Bet.Wallet.Application.DTOs;

namespace _67Bet.Wallet.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Guid userId, decimal amount, string currency = "PLN");
    Task<string?> CreatePayoutAsync(Guid userId, decimal amount, string currency = "pln");
    Task<bool> HandleWebhookAsync(string json, string signature);
}
