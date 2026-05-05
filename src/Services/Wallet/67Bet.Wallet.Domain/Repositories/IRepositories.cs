/*
 * Interfejsy repozytoriów dla modułu portfela (Wallet).
 * Kontrakty dla operacji na portfelach i historii transakcji.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Domain.Repositories;

public interface IWalletRepository : IRepository<_67Bet.Wallet.Domain.Entities.Wallet>
{
    Task<_67Bet.Wallet.Domain.Entities.Wallet?> GetByUserIdAsync(Guid userId);
}

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId);
}
