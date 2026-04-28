using System;
using System.Threading.Tasks;
using _67Bet.Wallet.Application.Services;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Wallet.Domain.Enums;
using _67Bet.Wallet.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class WalletServiceTests
{
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly WalletService _walletService;

    public WalletServiceTests()
    {
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _walletService = new WalletService(_walletRepositoryMock.Object, _transactionRepositoryMock.Object);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnZero_WhenWalletDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((_67Bet.Wallet.Domain.Entities.Wallet?)null);

        // Act
        var balance = await _walletService.GetBalanceAsync(userId);

        // Assert
        balance.Should().Be(0);
    }

    [Fact]
    public async Task GetBalanceAsync_ShouldReturnBalance_WhenWalletExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
        wallet.Deposit(100);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var balance = await _walletService.GetBalanceAsync(userId);

        // Assert
        balance.Should().Be(100);
    }

    [Fact]
    public async Task DepositAsync_ShouldIncreaseBalanceAndCreateTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        await _walletService.DepositAsync(userId, 50);

        // Assert
        wallet.Balance.Should().Be(50);
        _walletRepositoryMock.Verify(x => x.UpdateAsync(wallet), Times.Once);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t => 
            t.Amount == 50 && 
            t.Type == TransactionType.Deposit && 
            t.Status == TransactionStatus.Completed)), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldDecreaseBalance_WhenFundsAreSufficient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
        wallet.Deposit(100);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        await _walletService.WithdrawAsync(userId, 40);

        // Assert
        wallet.Balance.Should().Be(60);
        _walletRepositoryMock.Verify(x => x.UpdateAsync(wallet), Times.Once);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t => 
            t.Amount == 40 && 
            t.Type == TransactionType.Withdrawal && 
            t.Status == TransactionStatus.Completed)), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_ShouldThrowException_WhenWalletDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((_67Bet.Wallet.Domain.Entities.Wallet?)null);

        // Act & Assert
        await _walletService.Invoking(s => s.WithdrawAsync(userId, 50))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Portfel nie istnieje.");
    }

    [Fact]
    public async Task ProcessStakeAsync_ShouldReturnTrueAndDecreaseBalance_WhenFundsAreSufficient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
        wallet.Deposit(100);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _walletService.ProcessStakeAsync(userId, 30);

        // Assert
        result.Should().BeTrue();
        wallet.Balance.Should().Be(70);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t => 
            t.Type == TransactionType.Stake)), Times.Once);
    }

    [Fact]
    public async Task ProcessStakeAsync_ShouldReturnFalse_WhenFundsAreInSufficient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
        wallet.Deposit(20);
        _walletRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(wallet);

        // Act
        var result = await _walletService.ProcessStakeAsync(userId, 30);

        // Assert
        result.Should().BeFalse();
        wallet.Balance.Should().Be(20);
        _transactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Transaction>()), Times.Never);
    }
}
