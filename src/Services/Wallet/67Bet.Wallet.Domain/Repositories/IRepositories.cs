/*
 * Interfejsy repozytoriĂłw dla moduĹ‚u portfela (Wallet).
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

public interface IReferralCodeRepository : IRepository<ReferralCode>
{
    Task<ReferralCode?> GetByCodeAsync(string code);
    Task<ReferralCode?> GetByUserIdAsync(Guid userId);
}

public interface IPromoCodeRepository : IRepository<PromoCode>
{
    Task<PromoCode?> GetByCodeAsync(string code);
    Task<IEnumerable<PromoCode>> GetAllActiveAsync();
}

public interface IUserCodeUsageRepository : IRepository<UserCodeUsage>
{
    Task<bool> HasUsedCodeAsync(Guid userId, Guid codeId);
    Task<bool> HasUsedAnyReferralAsync(Guid userId);
    Task<int> GetUsageCountForReferralAsync(Guid codeId);
    Task<UserCodeUsage?> GetUsedReferralAsync(Guid userId);
}
