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

    public async Task<AiMatchInsight?> GetByEventIdAsync(Guid eventId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(x => x.EventId == eventId);
    }
}
