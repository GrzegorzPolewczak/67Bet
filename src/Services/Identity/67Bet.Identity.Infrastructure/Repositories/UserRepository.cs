/*
 * Implementacja repozytorium UserRepository.
 * Odpowiada za operacje CRUD na danych użytkowników w systemie.
 */
using Microsoft.EntityFrameworkCore;
using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;
using System;
using System.Threading.Tasks;
using _67Bet.Identity.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.Identity.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<KycSession> KycSessions => Set<KycSession>();

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.HasKey(u => u.Id);
                builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
                builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
                builder.HasIndex(u => u.Username).IsUnique();
                builder.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<KycSession>(builder =>
            {
                builder.HasKey(k => k.Id);
                builder.Property(k => k.Status).IsRequired();
            });
        }
    }
}

namespace _67Bet.Identity.Infrastructure.Repositories
{
    public class UserRepository : EFRepository<User, IdentityDbContext>, IUserRepository
    {
        public UserRepository(IdentityDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
