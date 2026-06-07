/*
 * Implementacje repozytoriów dla modułu Betting przy użyciu Entity Framework Core.
 * Serwisy te odpowiadają za bezpośrednią komunikację z bazą danych PostgreSQL.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Infrastructure.Repositories;

public class SportRepository : EFRepository<Sport, BettingDbContext>, ISportRepository
{
    public SportRepository(BettingDbContext context) : base(context) { }
}

public class EventRepository : EFRepository<Event, BettingDbContext>, IEventRepository
{
    public EventRepository(BettingDbContext context) : base(context) { }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync()
    {
        return await _dbSet
            .Include(e => e.Markets)
                .ThenInclude(m => m.Outcomes)
            .Where(e => e.Status == EventStatus.Scheduled || e.Status == EventStatus.Live)
            .ToListAsync();
    }
}

public class MarketRepository : EFRepository<Market, BettingDbContext>, IMarketRepository
{
    public MarketRepository(BettingDbContext context) : base(context) { }

    public override async Task<Market?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public override async Task<IEnumerable<Market>> GetAllAsync()
    {
        return await _dbSet
            .Include(m => m.Outcomes)
            .ToListAsync();
    }

    public async Task<IEnumerable<Market>> GetByEventIdAsync(Guid eventId)
    {
        return await _dbSet
            .Include(m => m.Outcomes)
            .Where(m => m.EventId == eventId)
            .ToListAsync();
    }
}

public class TicketRepository : EFRepository<Ticket, BettingDbContext>, ITicketRepository
{
    public TicketRepository(BettingDbContext context) : base(context) { }

    public override async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Bets)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.Bets)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetActiveTicketsAsync()
    {
        return await _dbSet
            .Include(t => t.Bets)
            .Where(t => t.Status == TicketStatus.Pending)
            .ToListAsync();
    }
}
