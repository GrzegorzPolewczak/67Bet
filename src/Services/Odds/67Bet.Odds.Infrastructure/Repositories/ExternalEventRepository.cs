using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Odds.Domain.Entities;
using _67Bet.Odds.Domain.Repositories;
using _67Bet.Odds.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _67Bet.Odds.Infrastructure.Repositories;

public class ExternalEventRepository : IExternalEventRepository
{
    private readonly OddsDbContext _context;

    public ExternalEventRepository(OddsDbContext context)
    {
        _context = context;
    }

    public async Task<ExternalEvent?> GetByIdAsync(Guid id)
    {
        return await _context.ExternalEvents
            .Include(e => e.Markets)
            .ThenInclude(m => m.Outcomes)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<ExternalEvent?> GetByExternalIdAsync(string externalId)
    {
        return await _context.ExternalEvents
            .Include(e => e.Markets)
            .ThenInclude(m => m.Outcomes)
            .FirstOrDefaultAsync(e => e.ExternalId == externalId);
    }

    public async Task<IEnumerable<ExternalEvent>> GetAllActiveAsync()
    {
        return await _context.ExternalEvents
            .Include(e => e.Markets)
            .ThenInclude(m => m.Outcomes)
            .Where(e => e.StartTime > DateTime.UtcNow.AddHours(-2))
            .ToListAsync();
    }

    public async Task AddAsync(ExternalEvent @event)
    {
        await _context.ExternalEvents.AddAsync(@event);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ExternalEvent @event)
    {
        _context.ExternalEvents.Update(@event);
        await _context.SaveChangesAsync();
    }
}
