using System;
using WalletEntity = _67Bet.Wallet.Domain.Entities.Wallet;
using FluentAssertions;
using Xunit;

namespace _67Bet.UnitTests.Domain;

public class WalletTests
{
    [Fact]
    public void Deposit_ShouldIncreaseBalance_WhenAmountIsPositive()
    {
        // Arrange
        var wallet = new WalletEntity(Guid.NewGuid());
        var initialBalance = wallet.Balance;
        var depositAmount = 100m;

        // Act
        wallet.Deposit(depositAmount);

        // Assert
        wallet.Balance.Should().Be(initialBalance + depositAmount);
    }

    [Fact]
    public void Deposit_ShouldThrowArgumentException_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var wallet = new WalletEntity(Guid.NewGuid());

        // Act & Assert
        wallet.Invoking(w => w.Deposit(0))
            .Should().Throw<ArgumentException>()
            .WithMessage("Amount must be positive.");

        wallet.Invoking(w => w.Deposit(-10))
            .Should().Throw<ArgumentException>()
            .WithMessage("Amount must be positive.");
    }

    [Fact]
    public void Withdraw_ShouldDecreaseBalance_WhenFundsAreSufficient()
    {
        // Arrange
        var wallet = new WalletEntity(Guid.NewGuid());
        wallet.Deposit(100m);
        var initialBalance = wallet.Balance;
        var withdrawAmount = 40m;

        // Act
        wallet.Withdraw(withdrawAmount);

        // Assert
        wallet.Balance.Should().Be(initialBalance - withdrawAmount);
    }

    [Fact]
    public void Withdraw_ShouldThrowInvalidOperationException_WhenFundsAreInsufficient()
    {
        // Arrange
        var wallet = new WalletEntity(Guid.NewGuid());
        wallet.Deposit(20m);

        // Act & Assert
        wallet.Invoking(w => w.Withdraw(30m))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient funds.");
    }

    [Fact]
    public void Withdraw_ShouldThrowArgumentException_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        var wallet = new WalletEntity(Guid.NewGuid());

        // Act & Assert
        wallet.Invoking(w => w.Withdraw(0))
            .Should().Throw<ArgumentException>()
            .WithMessage("Amount must be positive.");

        wallet.Invoking(w => w.Withdraw(-10))
            .Should().Throw<ArgumentException>()
            .WithMessage("Amount must be positive.");
    }
}
