/*
 * Implementacje repozytoriów dla modułu Wallet.
 * Obsługuje zapis transakcji oraz aktualizację salda portfeli użytkowników.
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

        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WalletEntity>(builder =>
            {
                builder.HasKey(w => w.Id);
                builder.Property(w => w.Balance).HasPrecision(18, 2);
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
}
