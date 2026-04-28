/*
 * Interfejsy repozytoriów dla modułu portfela (Wallet).
 * Kontrakty dla operacji na portfelach i historii transakcji.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _67Bet.Wallet.Domain.Repositories
{
    using _67Bet.Wallet.Domain.Entities;

    public interface IWalletRepository
    {
        Task<Wallet?> GetByUserIdAsync(Guid userId);
        Task<Wallet?> GetByIdAsync(Guid id);
        Task AddAsync(Wallet wallet);
        Task UpdateAsync(Wallet wallet);
    }

    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId);
        Task AddAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
    }
}

