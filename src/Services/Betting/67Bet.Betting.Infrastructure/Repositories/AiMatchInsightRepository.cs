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

    public async Task AddOrUpdateAsync(AiMatchInsight insight)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(x => x.EventId == insight.EventId);

        if (existing == null)
        {
            await _dbSet.AddAsync(insight);
        }
        else
        {
            _context.Entry(existing).CurrentValues.SetValues(insight);
        }

        await _context.SaveChangesAsync();
    }
}
