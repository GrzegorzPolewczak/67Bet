using Microsoft.EntityFrameworkCore;
using _67Bet.CustomBet.Domain.Entities;
using _67Bet.CustomBet.Domain.Repositories;
using _67Bet.CustomBet.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                builder.Property(r => r.AdminFinalOdds).HasPrecision(10, 2);
            });
        }
    }
}

namespace _67Bet.CustomBet.Infrastructure.Repositories
{
    using _67Bet.CustomBet.Infrastructure.Persistence;

    public class CustomBetRepository : ICustomBetRepository
    {
        private readonly CustomBetDbContext _context;

        public CustomBetRepository(CustomBetDbContext context)
        {
            _context = context;
        }

        public async Task<CustomBetRequest?> GetByIdAsync(Guid id) => await _context.CustomBetRequests.FindAsync(id);

        public async Task<IEnumerable<CustomBetRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.CustomBetRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<CustomBetRequest>> GetPendingRequestsAsync()
        {
            return await _context.CustomBetRequests
                .Where(r => r.Status == RequestStatus.Pending || r.Status == RequestStatus.Reviewing)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(CustomBetRequest request)
        {
            await _context.CustomBetRequests.AddAsync(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomBetRequest request)
        {
            _context.CustomBetRequests.Update(request);
            await _context.SaveChangesAsync();
        }
    }
}
