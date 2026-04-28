using System;
using System.Threading.Tasks;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Wallet.Domain.Repositories;
using _67Bet.Wallet.Domain.Enums;

namespace _67Bet.Wallet.Application.Services;

/*
 * Serwis WalletService implementuje logikę zarządzania środkami finansowymi.
 * Zapewnia spójność operacji na saldzie (wpłaty/wypłaty) oraz rejestruje historię transakcji.
 */
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;

    public WalletService(IWalletRepository walletRepository, ITransactionRepository transactionRepository)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        return wallet?.Balance ?? 0;
    }

    public async Task DepositAsync(Guid userId, decimal amount)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Deposit(amount);

        var transaction = new Transaction(wallet.Id, amount, TransactionType.Deposit);
        transaction.Complete();

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
    }

    public async Task WithdrawAsync(Guid userId, decimal amount)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null) throw new InvalidOperationException("Portfel nie istnieje.");

        wallet.Withdraw(amount);

        var transaction = new Transaction(wallet.Id, amount, TransactionType.Withdrawal);
        transaction.Complete();

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
    }

    public async Task<bool> ProcessStakeAsync(Guid userId, decimal amount)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null || wallet.Balance < amount) return false;

        wallet.Withdraw(amount);

        var transaction = new Transaction(wallet.Id, amount, TransactionType.Stake);
        transaction.Complete();

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
        return true;
    }

    public async Task ProcessPayoutAsync(Guid userId, decimal amount)
    {
        var wallet = await GetOrCreateWalletAsync(userId);
        wallet.Deposit(amount);

        var transaction = new Transaction(wallet.Id, amount, TransactionType.Payout);
        transaction.Complete();

        await _walletRepository.UpdateAsync(wallet);
        await _transactionRepository.AddAsync(transaction);
    }

    public async Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetWalletByUserIdAsync(Guid userId)
    {
        return await _walletRepository.GetByUserIdAsync(userId);
    }

    private async Task<_67Bet.Wallet.Domain.Entities.Wallet> GetOrCreateWalletAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet == null)
        {
            wallet = new _67Bet.Wallet.Domain.Entities.Wallet(userId);
            await _walletRepository.AddAsync(wallet);
        }
        return wallet;
    }
}
