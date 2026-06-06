/*
 * Implementacje repozytoriĂłw dla moduĹ‚u Wallet.
 * ObsĹ‚uguje zapis transakcji oraz aktualizacjÄ™ salda portfeli uĹĽytkownikĂłw.
 */
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Wallet.Domain.Repositories;
using _67Bet.Wallet.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Infrastructure.Persistence
{
    using WalletEntity = _67Bet.Wallet.Domain.Entities.Wallet;

    public class WalletDbContext : DbContext
    {
        public DbSet<WalletEntity> Wallets => Set<WalletEntity>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
        public DbSet<UserCodeUsage> UserCodeUsages => Set<UserCodeUsage>();

        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WalletEntity>(builder =>
            {
                builder.HasKey(w => w.Id);
                builder.Property(w => w.Balance).HasPrecision(18, 2);
                builder.Property(w => w.FreebetBalance).HasPrecision(18, 2);
                builder.Property(w => w.Currency).HasMaxLength(3);
                builder.Property(w => w.Version).IsConcurrencyToken();
                builder.HasIndex(w => w.UserId).IsUnique();
            });

            modelBuilder.Entity<Transaction>(builder =>
            {
                builder.HasKey(t => t.Id);
                builder.Property(t => t.Amount).HasPrecision(18, 2);
                builder.HasOne<WalletEntity>().WithMany().HasForeignKey(t => t.WalletId);
            });

            modelBuilder.Entity<ReferralCode>(builder =>
            {
                builder.HasKey(rc => rc.Id);
                builder.Property(rc => rc.Code).HasMaxLength(10).IsRequired();
                builder.HasIndex(rc => rc.Code).IsUnique();
                builder.HasIndex(rc => rc.UserId).IsUnique();
            });

            modelBuilder.Entity<PromoCode>(builder =>
            {
                builder.HasKey(pc => pc.Id);
                builder.Property(pc => pc.Code).IsRequired();
                builder.Property(pc => pc.RewardAmount).HasPrecision(18, 2);
                builder.HasIndex(pc => pc.Code).IsUnique();
            });

            modelBuilder.Entity<UserCodeUsage>(builder =>
            {
                builder.HasKey(ucu => ucu.Id);
                builder.HasIndex(ucu => new { ucu.UserId, ucu.CodeId }).IsUnique();
            });
        }
    }
}

namespace _67Bet.Wallet.Infrastructure.Repositories
{
    using WalletEntity = _67Bet.Wallet.Domain.Entities.Wallet;

    public class WalletRepository : EFRepository<WalletEntity, WalletDbContext>, IWalletRepository
    {
        public WalletRepository(WalletDbContext context) : base(context) { }

        public async Task<WalletEntity?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId);
        }
    }

    public class TransactionRepository : EFRepository<Transaction, WalletDbContext>, ITransactionRepository
    {
        public TransactionRepository(WalletDbContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _dbSet
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
    }

    public class ReferralCodeRepository : EFRepository<ReferralCode, WalletDbContext>, IReferralCodeRepository
    {
        public ReferralCodeRepository(WalletDbContext context) : base(context) { }

        public async Task<ReferralCode?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(rc => rc.Code == code.ToUpper());
        }

        public async Task<ReferralCode?> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.FirstOrDefaultAsync(rc => rc.UserId == userId);
        }
    }

    public class PromoCodeRepository : EFRepository<PromoCode, WalletDbContext>, IPromoCodeRepository
    {
        public PromoCodeRepository(WalletDbContext context) : base(context) { }

        public async Task<PromoCode?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(pc => pc.Code == code.ToUpper());
        }

        public async Task<IEnumerable<PromoCode>> GetAllActiveAsync()
        {
            return await _dbSet.Where(pc => pc.IsActive).ToListAsync();
        }
    }

    public class UserCodeUsageRepository : EFRepository<UserCodeUsage, WalletDbContext>, IUserCodeUsageRepository
    {
        public UserCodeUsageRepository(WalletDbContext context) : base(context) { }

        public async Task<bool> HasUsedCodeAsync(Guid userId, Guid codeId)
        {
            return await _dbSet.AnyAsync(ucu => ucu.UserId == userId && ucu.CodeId == codeId);
        }

        public async Task<bool> HasUsedAnyReferralAsync(Guid userId)
        {
            return await _dbSet.AnyAsync(ucu => ucu.UserId == userId && ucu.IsReferral);
        }

        public async Task<int> GetUsageCountForReferralAsync(Guid codeId)
        {
            return await _dbSet.CountAsync(ucu => ucu.CodeId == codeId);
        }
    }
}
