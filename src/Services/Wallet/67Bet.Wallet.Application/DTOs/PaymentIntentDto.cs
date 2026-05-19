namespace _67Bet.Wallet.Application.DTOs;

public record CreatePaymentIntentRequest(decimal Amount, string Currency = "PLN");

public record PaymentIntentResponseDto(string ClientSecret, string PublishableKey);
