/*
 * Interfejsy repozytoriów dla modułu Betting.
 * Definiują kontrakty dla operacji na sportach, wydarzeniach, rynkach i kuponach.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Domain.Entities;

namespace _67Bet.Betting.Domain.Repositories;

public interface ISportRepository
{
    Task<Sport?> GetByIdAsync(Guid id);
    Task<IEnumerable<Sport>> GetAllAsync();
    Task AddAsync(Sport sport);
}

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<IEnumerable<Event>> GetActiveEventsAsync();
    Task AddAsync(Event @event);
    Task UpdateAsync(Event @event);
}

public interface IMarketRepository
{
    Task<Market?> GetByIdAsync(Guid id);
    Task<IEnumerable<Market>> GetByEventIdAsync(Guid eventId);
    Task AddAsync(Market market);
}

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
}

