using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Entities.VirtualRacing;

namespace _67Bet.Betting.Application.Services;

/*
 * Serwis BettingService implementuje logikę biznesową modułu zakładów.
 * Odpowiada za weryfikację kursów, tworzenie kuponów (w tym AKO) oraz proces rozliczania zdarzeń.
 */
public class BettingService : IBettingService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IVirtualRaceRepository _virtualRaceRepository;

    public BettingService(
        IEventRepository eventRepository,
        IMarketRepository marketRepository,
        ITicketRepository ticketRepository,
        IVirtualRaceRepository virtualRaceRepository)
    {
        _eventRepository = eventRepository;
        _marketRepository = marketRepository;
        _ticketRepository = ticketRepository;
        _virtualRaceRepository = virtualRaceRepository;
    }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync()
    {
        return await _eventRepository.GetActiveEventsAsync();
    }

    public async Task<Ticket> PlaceTicketAsync(Guid userId, decimal stake, IEnumerable<Guid> outcomeIds)
    {
        if (stake <= 0)
            throw new ArgumentException("Stawka musi być większa od zera.");

        if (outcomeIds == null || !outcomeIds.Any())
            throw new ArgumentException("Kupon musi zawierać przynajmniej jeden zakład.");

        var ticket = new Ticket(userId, stake);

        foreach (var outcomeId in outcomeIds)
        {
            // W wersji I szukamy outcome w marketach (uproszczenie)
            // W produkcyjnej wersji warto mieć dedykowane repozytorium dla Outcome lub lepszy mechanizm wyszukiwania
            var events = await _eventRepository.GetActiveEventsAsync();
            Outcome? foundOutcome = null;
            
            foreach (var @event in events)
            {
                var markets = await _marketRepository.GetByEventIdAsync(@event.Id);
                foreach (var market in markets)
                {
                    if (!market.IsActive) continue;
                    
                    var outcome = market.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
                    if (outcome != null)
                    {
                        foundOutcome = outcome;
                        break;
                    }
                }
                if (foundOutcome != null) break;
            }

            if (foundOutcome != null)
            {
                ticket.AddBet(foundOutcome.Id, foundOutcome.CurrentPrice);
            }
            else
            {
                // Sprawdzamy czy to przypadek wirtualnych wyścigów
                var virtualRaces = await _virtualRaceRepository.GetActiveRacesAsync();
                VirtualRaceParticipant? foundVirtualParticipant = null;
                
                foreach (var race in virtualRaces)
                {
                    var participant = race.Participants.FirstOrDefault(p => p.Id == outcomeId);
                    if (participant != null)
                    {
                        foundVirtualParticipant = participant;
                        break;
                    }
                }

                if (foundVirtualParticipant != null)
                {
                    ticket.AddBet(foundVirtualParticipant.Id, foundVirtualParticipant.Odds);
                }
                else
                {
                    throw new InvalidOperationException($"Nie znaleziono aktywnego wyniku lub uczestnika wirtualnego wyścigu o ID: {outcomeId}");
                }
            }
        }

        await _ticketRepository.AddAsync(ticket);
        return ticket;
    }

    public async Task SettleEventAsync(Guid eventId, List<Guid> winningOutcomeIds)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) throw new InvalidOperationException("Wydarzenie nie istnieje.");

        @event.UpdateStatus(EventStatus.Finished);
        await _eventRepository.UpdateAsync(@event);

        // Pobierz wszystkie rynki dla tego wydarzenia
        var markets = await _marketRepository.GetByEventIdAsync(eventId);
        foreach (var market in markets)
        {
            foreach (var outcome in market.Outcomes)
            {
                bool isWinner = winningOutcomeIds.Contains(outcome.Id);
                outcome.SetResult(isWinner);
            }
            // Zmiany w outcome'ach powinny być zapisane przez update marketu lub bezpośrednio
            // Tutaj polegamy na mechanizmie śledzenia zmian w DbContext (implikowane przez Repository)
        }
        
        // Logika rozliczania kuponów (Settlement Engine) powinna być wywołana tutaj
        // Dla uproszczenia w wersji I możemy zostawić to jako osobny proces lub rozliczyć tutaj proste kupony
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
    {
        return await _ticketRepository.GetByIdAsync(ticketId);
    }

    public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(Guid userId)
    {
        return await _ticketRepository.GetByUserIdAsync(userId);
    }
}
