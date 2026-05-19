using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.Services;
using _67Bet.Wallet.Infrastructure.Services;
using Xunit;
using FluentAssertions;

namespace _67Bet.UnitTests.Services;

public class StripePaymentServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly StripePaymentService _service;

    public StripePaymentServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<StripePaymentService>>();
        _walletServiceMock = new Mock<IWalletService>();

        _configurationMock.Setup(x => x["Stripe:SecretKey"]).Returns("sk_test_123");
        
        _service = new StripePaymentService(
            _configurationMock.Object,
            _loggerMock.Object,
            _walletServiceMock.Object);
    }

    [Fact]
    public async Task ProcessEventAsync_ShouldDepositFunds_WhenPaymentIntentSucceeded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var amount = 10000L; // 100.00 PLN
        var paymentIntent = new PaymentIntent
        {
            Amount = amount,
            Currency = "pln",
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() }
            }
        };

        var stripeEvent = new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = paymentIntent
            }
        };

        // Act
        var result = await _service.ProcessEventAsync(stripeEvent);

        // Assert
        result.Should().BeTrue();
        _walletServiceMock.Verify(x => x.DepositAsync(userId, 100.00m), Times.Once);
    }

    [Fact]
    public async Task ProcessEventAsync_ShouldNotDepositFunds_WhenUserIdIsMissing()
    {
        // Arrange
        var paymentIntent = new PaymentIntent
        {
            Amount = 10000,
            Currency = "pln",
            Metadata = new Dictionary<string, string>()
        };

        var stripeEvent = new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = paymentIntent
            }
        };

        // Act
        var result = await _service.ProcessEventAsync(stripeEvent);

        // Assert
        result.Should().BeTrue();
        _walletServiceMock.Verify(x => x.DepositAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ProcessEventAsync_ShouldIgnoreOtherEventTypes()
    {
        // Arrange
        var stripeEvent = new Event
        {
            Type = "customer.created"
        };

        // Act
        var result = await _service.ProcessEventAsync(stripeEvent);

        // Assert
        result.Should().BeTrue();
        _walletServiceMock.Verify(x => x.DepositAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }
}
