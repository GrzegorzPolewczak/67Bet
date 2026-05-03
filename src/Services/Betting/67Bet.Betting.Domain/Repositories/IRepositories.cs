/*
 * Interfejsy repozytoriów dla modułu Betting.
 * Definiują kontrakty dla operacji na sportach, wydarzeniach, rynkach i kuponach.
 */
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Repositories;

public interface ISportRepository : IRepository<Sport>
{
}

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetActiveEventsAsync();
}

public interface IMarketRepository : IRepository<Market>
{
    Task<IEnumerable<Market>> GetByEventIdAsync(Guid eventId);
}

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId);
}
