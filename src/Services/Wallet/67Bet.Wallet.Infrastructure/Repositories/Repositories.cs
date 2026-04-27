using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _67Bet.Wallet.Infrastructure.Persistence
{
    using _67Bet.Wallet.Domain.Entities;

    public class WalletDbContext : DbContext
    {
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();

        public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Wallet>(builder =>
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
                builder.HasOne<Wallet>().WithMany().HasForeignKey(t => t.WalletId);
            });
        }
    }
}

namespace _67Bet.Wallet.Infrastructure.Repositories
{
    using _67Bet.Wallet.Infrastructure.Persistence;
    using _67Bet.Wallet.Domain.Repositories;
    using WalletEntity = _67Bet.Wallet.Domain.Entities.Wallet;
    using TransactionEntity = _67Bet.Wallet.Domain.Entities.Transaction;

    public class WalletRepository : IWalletRepository
    {
        private readonly WalletDbContext _context;

        public WalletRepository(WalletDbContext context)
        {
            _context = context;
        }

        public async Task<WalletEntity?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<WalletEntity?> GetByIdAsync(Guid id) => await _context.Wallets.FindAsync(id);

        public async Task AddAsync(WalletEntity wallet)
        {
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WalletEntity wallet)
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
        }
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly WalletDbContext _context;

        public TransactionRepository(WalletDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionEntity?> GetByIdAsync(Guid id) => await _context.Transactions.FindAsync(id);

        public async Task<IEnumerable<TransactionEntity>> GetByWalletIdAsync(Guid walletId)
        {
            return await _context.Transactions
                .Where(t => t.WalletId == walletId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(TransactionEntity transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TransactionEntity transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
