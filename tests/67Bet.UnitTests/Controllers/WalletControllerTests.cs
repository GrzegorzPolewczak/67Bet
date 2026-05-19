using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using _67Bet.Wallet.Api.Controllers;
using _67Bet.Wallet.Application.DTOs;
using _67Bet.Wallet.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Controllers;

public class WalletControllerTests
{
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly WalletController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public WalletControllerTests()
    {
        _walletServiceMock = new Mock<IWalletService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _controller = new WalletController(_walletServiceMock.Object, _paymentServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task CreatePaymentIntent_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var request = new CreatePaymentIntentRequest(100, "PLN");
        var expectedResponse = new PaymentIntentResponseDto("secret", "pk");
        _paymentServiceMock.Setup(x => x.CreatePaymentIntentAsync(_userId, 100, "PLN"))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreatePaymentIntent(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task CreatePaymentIntent_ShouldReturnBadRequest_WhenAmountIsZero()
    {
        // Arrange
        var request = new CreatePaymentIntentRequest(0, "PLN");

        // Act
        var result = await _controller.CreatePaymentIntent(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Withdraw_ShouldReturnNoContent_WhenBalanceIsSufficient()
    {
        // Arrange
        var request = new WithdrawRequest(50);
        _walletServiceMock.Setup(x => x.GetBalanceAsync(_userId)).ReturnsAsync(100);
        _paymentServiceMock.Setup(x => x.CreatePayoutAsync(_userId, 50, "pln")).ReturnsAsync((string?)null);

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _walletServiceMock.Verify(x => x.WithdrawAsync(_userId, 50), Times.Once);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnBadRequest_WhenBalanceIsInsufficient()
    {
        // Arrange
        var request = new WithdrawRequest(150);
        _walletServiceMock.Setup(x => x.GetBalanceAsync(_userId)).ReturnsAsync(100);

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _paymentServiceMock.Verify(x => x.CreatePayoutAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Withdraw_ShouldReturnBadRequest_WhenStripePayoutFails()
    {
        // Arrange
        var request = new WithdrawRequest(50);
        _walletServiceMock.Setup(x => x.GetBalanceAsync(_userId)).ReturnsAsync(100);
        _paymentServiceMock.Setup(x => x.CreatePayoutAsync(_userId, 50, "pln")).ReturnsAsync("Stripe error");

        // Act
        var result = await _controller.Withdraw(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.ToString().Should().Contain("Stripe error");
        _walletServiceMock.Verify(x => x.WithdrawAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task Webhook_ShouldReturnOk_WhenServiceReturnsTrue()
    {
        // Arrange
        var json = "{\"id\": \"evt_123\"}";
        var signature = "sig_123";
        _controller.Request.Headers["Stripe-Signature"] = signature;
        _controller.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        _paymentServiceMock.Setup(x => x.HandleWebhookAsync(It.IsAny<string>(), signature))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Webhook();

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Webhook_ShouldReturnBadRequest_WhenServiceReturnsFalse()
    {
        // Arrange
        var json = "{\"id\": \"evt_123\"}";
        var signature = "sig_123";
        _controller.Request.Headers["Stripe-Signature"] = signature;
        _controller.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

        _paymentServiceMock.Setup(x => x.HandleWebhookAsync(It.IsAny<string>(), signature))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Webhook();

        // Assert
        result.Should().BeOfType<BadRequestResult>();
    }
}
