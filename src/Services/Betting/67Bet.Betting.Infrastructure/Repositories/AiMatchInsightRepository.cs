using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;
using Microsoft.EntityFrameworkCore;

namespace _67Bet.Betting.Infrastructure.Repositories;

public class AiMatchInsightRepository : EFRepository<AiMatchInsight, BettingDbContext>, IAiMatchInsightRepository
{
    public AiMatchInsightRepository(BettingDbContext context) : base(context)
    {
    }

    public async Task<AiMatchInsight?> GetByEventIdAsync(string eventId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.EventId == eventId);
    }

    public override async Task<IEnumerable<AiMatchInsight>> GetAllAsync()
    {
        return await _dbSet
            .OrderByDescending(x => x.GeneratedAt)
            .ToListAsync();
    }

    public async Task AddOrUpdateAsync(AiMatchInsight insight)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(x => x.EventId == insight.EventId);

        if (existing == null)
        {
            await _dbSet.AddAsync(insight);
        }
        else
        {
            existing.UpdateInsight(insight.Content);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string eventId)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(x => x.EventId == eventId);
        if (existing != null)
        {
            _dbSet.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddLogAsync(AiGenerationLog log)
    {
        await _context.Set<AiGenerationLog>().AddAsync(log);
        await _context.SaveChangesAsync();
    }
}
