/*
 * Implementacja repozytorium CustomBetRepository.
 * Zarządza trwałym zapisem i odczytem wniosków o zakłady specjalne w bazie danych.
 */
using Microsoft.EntityFrameworkCore;
using _67Bet.CustomBet.Domain.Entities;
using _67Bet.CustomBet.Domain.Repositories;
using _67Bet.CustomBet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.CustomBet.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.CustomBet.Infrastructure.Persistence
{
    public class CustomBetDbContext : DbContext
    {
        public DbSet<CustomBetRequest> CustomBetRequests => Set<CustomBetRequest>();

        public CustomBetDbContext(DbContextOptions<CustomBetDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomBetRequest>(builder =>
            {
                builder.HasKey(r => r.Id);
                builder.Property(r => r.Description).IsRequired();
                builder.Property(r => r.AiSuggestedOdds).HasPrecision(10, 2);
                builder.Property(r => r.AiAnalysisNote).HasColumnType("longtext");
                builder.Property(r => r.AiRiskLevel).HasMaxLength(50);
                builder.Property(r => r.AiCategory).HasMaxLength(100);
                builder.Property(r => r.AdminFinalOdds).HasPrecision(10, 2);
            });
        }
    }
}

namespace _67Bet.CustomBet.Infrastructure.Repositories
{
    public class CustomBetRepository : EFRepository<CustomBetRequest, CustomBetDbContext>, ICustomBetRepository
    {
        public CustomBetRepository(CustomBetDbContext context) : base(context) { }

        public async Task<IEnumerable<CustomBetRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomBetRequest>> GetPendingRequestsAsync()
        {
            return await _dbSet
                .Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.Reviewing)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
