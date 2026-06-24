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
    Task<Sport?> GetByNameAsync(string name);
}

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetActiveEventsAsync();
    Task<Event?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Event>> GetPastUnsettledEventsAsync();
}

public interface IMarketRepository : IRepository<Market>
{
    Task<IEnumerable<Market>> GetByEventIdAsync(Guid eventId);
}

public interface ITicketRepository : IRepository<Ticket>
{
    Task<IEnumerable<Ticket>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetActiveTicketsAsync();
}

public interface IAiMatchInsightRepository : IRepository<AiMatchInsight>
{
    Task<AiMatchInsight?> GetByEventIdAsync(string eventId);
    Task<IEnumerable<AiMatchInsight>> GetAllAsync();
    Task AddOrUpdateAsync(AiMatchInsight insight);
    Task DeleteAsync(string eventId);
    Task AddLogAsync(AiGenerationLog log);
}
