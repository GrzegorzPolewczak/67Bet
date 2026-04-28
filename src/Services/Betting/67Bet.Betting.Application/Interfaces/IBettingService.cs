using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Domain.Entities;

namespace _67Bet.Betting.Application.Interfaces;

/*
 * Interfejs IBettingService definiuje główne operacje biznesowe związane z obstawianiem,
 * takie jak stawianie kuponów, pobieranie aktywnych wydarzeń oraz rozliczanie wyników.
 */
public interface IBettingService
{
    Task<IEnumerable<Event>> GetActiveEventsAsync();
    Task<Ticket> PlaceTicketAsync(Guid userId, decimal stake, IEnumerable<Guid> outcomeIds);
    Task SettleEventAsync(Guid eventId, List<Guid> winningOutcomeIds);
    Task<Ticket?> GetTicketByIdAsync(Guid ticketId);
    Task<IEnumerable<Ticket>> GetUserTicketsAsync(Guid userId);
}
