using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using _67Bet.Wallet.Application.DTOs;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.Services;

namespace _67Bet.Wallet.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;
    private readonly IWalletService _walletService;

    public StripePaymentService(
        IConfiguration configuration, 
        ILogger<StripePaymentService> logger,
        IWalletService walletService)
    {
        _configuration = configuration;
        _logger = logger;
        _walletService = walletService;
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(Guid userId, decimal amount, string currency = "PLN")
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), 
            Currency = currency.ToLower(),
            PaymentMethodTypes = new List<string> { "card", "blik" },
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return new PaymentIntentResponseDto(
            intent.ClientSecret, 
            _configuration["Stripe:PublishableKey"] ?? string.Empty);
    }

    public async Task<string?> CreatePayoutAsync(Guid userId, decimal amount, string currency = "pln")
    {
        try
        {
            var options = new PayoutCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency.ToLower(),
                Method = "standard", // Zmienione z "instant" na "standard" dla lepszej kompatybilności
                Description = $"Withdrawal for User: {userId}",
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() }
                }
            };

            var service = new PayoutService();
            await service.CreateAsync(options);
            return null; // Brak błędu
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe Payout error for user {UserId}: {Message}", userId, e.Message);
            return e.Message; // Zwróć treść błędu ze Stripe
        }
    }

    public async Task<bool> HandleWebhookAsync(string json, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signature,
                _configuration["Stripe:WebhookSecret"]
            );

            return await ProcessEventAsync(stripeEvent);
        }
        catch (StripeException e)
        {
            _logger.LogError(e, "Stripe webhook error");
            return false;
        }
    }

    public async Task<bool> ProcessEventAsync(Event stripeEvent)
    {
        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent != null)
            {
                if (paymentIntent.Metadata.TryGetValue("UserId", out var userIdString) && 
                    Guid.TryParse(userIdString, out var userId))
                {
                    var amount = paymentIntent.Amount / 100m;
                    await _walletService.DepositAsync(userId, amount);
                    _logger.LogInformation("Successfully processed payment of {Amount} {Currency} for user {UserId}", amount, paymentIntent.Currency, userId);
                }
                else
                {
                    _logger.LogWarning("PaymentIntent succeeded but UserId was not found in metadata. IntentId: {IntentId}", paymentIntent.Id);
                }
            }
        }

        return true;
    }
}
