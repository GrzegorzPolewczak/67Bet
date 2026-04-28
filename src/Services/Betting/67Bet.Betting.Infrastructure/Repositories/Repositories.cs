using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Infrastructure.Persistence;

namespace _67Bet.Betting.Infrastructure.Repositories;

public class SportRepository : ISportRepository
{
    private readonly BettingDbContext _context;

    public SportRepository(BettingDbContext context)
    {
        _context = context;
    }

    public async Task<Sport?> GetByIdAsync(Guid id) => await _context.Sports.FindAsync(id);

    public async Task<IEnumerable<Sport>> GetAllAsync() => await _context.Sports.ToListAsync();

    public async Task AddAsync(Sport sport)
    {
        await _context.Sports.AddAsync(sport);
        await _context.SaveChangesAsync();
    }
}

public class EventRepository : IEventRepository
{
    private readonly BettingDbContext _context;

    public EventRepository(BettingDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id) => await _context.Events.FindAsync(id);

    public async Task<IEnumerable<Event>> GetActiveEventsAsync()
    {
        return await _context.Events
            .Where(e => e.Status == EventStatus.Scheduled || e.Status == EventStatus.Live)
            .ToListAsync();
    }

    public async Task AddAsync(Event @event)
    {
        await _context.Events.AddAsync(@event);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event @event)
    {
        _context.Events.Update(@event);
        await _context.SaveChangesAsync();
    }
}

public class MarketRepository : IMarketRepository
{
    private readonly BettingDbContext _context;

    public MarketRepository(BettingDbContext context)
    {
        _context = context;
    }

    public async Task<Market?> GetByIdAsync(Guid id)
    {
        return await _context.Markets
            .Include(m => m.Outcomes)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Market>> GetByEventIdAsync(Guid eventId)
    {
        return await _context.Markets
            .Include(m => m.Outcomes)
            .Where(m => m.EventId == eventId)
            .ToListAsync();
    }

    public async Task AddAsync(Market market)
    {
        await _context.Markets.AddAsync(market);
        await _context.SaveChangesAsync();
    }
}

public class TicketRepository : ITicketRepository
{
    private readonly BettingDbContext _context;

    public TicketRepository(BettingDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.Bets)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Tickets
            .Include(t => t.Bets)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }
}
