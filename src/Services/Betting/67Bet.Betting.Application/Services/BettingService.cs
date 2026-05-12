using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Wallet.Application.Interfaces;

namespace _67Bet.Betting.Application.Services;

/*
 * Serwis BettingService implementuje logikę biznesową modułu zakładów.
 * Odpowiada za weryfikację kursów, tworzenie kuponów (w tym AKO) oraz proces rozliczania zdarzeń.
 * Zintegrowany z WalletService w celu automatycznej obsługi stawek i wygranych.
 */
public class BettingService : IBettingService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IWalletService _walletService;

    public BettingService(
        IEventRepository eventRepository,
        IMarketRepository marketRepository,
        ITicketRepository ticketRepository,
        IWalletService walletService)
    {
        _eventRepository = eventRepository;
        _marketRepository = marketRepository;
        _ticketRepository = ticketRepository;
        _walletService = walletService;
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

        // 1. Sprawdzenie i pobranie środków z portfela
        var stakeProcessed = await _walletService.ProcessStakeAsync(userId, stake);
        if (!stakeProcessed)
            throw new InvalidOperationException("Niewystarczające środki na koncie użytkownika.");

        var ticket = new Ticket(userId, stake);

        // 2. Pobranie kursów i walidacja typów
        foreach (var outcomeId in outcomeIds)
        {
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

            if (foundOutcome == null)
            {
                // W przypadku błędu należałoby zwrócić środki (uproszczenie: throw exception)
                await _walletService.ProcessPayoutAsync(userId, stake);
                throw new InvalidOperationException($"Nie znaleziono aktywnego wyniku o ID: {outcomeId}");
            }

            ticket.AddBet(foundOutcome.Id, foundOutcome.CurrentPrice);
        }

        await _ticketRepository.AddAsync(ticket);
        return ticket;
    }

    public async Task SettleEventAsync(Guid eventId, List<Guid> winningOutcomeIds)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) throw new InvalidOperationException("Wydarzenie nie istnieje.");

        // 1. Aktualizacja statusu wydarzenia i wyników
        @event.UpdateStatus(EventStatus.Finished);
        await _eventRepository.UpdateAsync(@event);

        var markets = await _marketRepository.GetByEventIdAsync(eventId);
        foreach (var market in markets)
        {
            foreach (var outcome in market.Outcomes)
            {
                bool isWinner = winningOutcomeIds.Contains(outcome.Id);
                outcome.SetResult(isWinner);
            }
        }
        
        // 2. Logika Settlement Engine - Rozliczanie kuponów
        // Pobieramy wszystkie kupony, które zawierają to wydarzenie (uproszczenie: wszystkie aktywne)
        var tickets = await _ticketRepository.GetActiveTicketsAsync();
        
        foreach (var ticket in tickets)
        {
            // Sprawdzamy tylko te kupony, które mają typy z tego wydarzenia
            bool ticketHasThisEvent = false;
            foreach (var bet in ticket.Bets)
            {
                if (winningOutcomeIds.Contains(bet.OutcomeId) || 
                    markets.Any(m => m.Outcomes.Any(o => o.Id == bet.OutcomeId)))
                {
                    ticketHasThisEvent = true;
                    break;
                }
            }

            if (!ticketHasThisEvent) continue;

            // Logika AKO: Kupon jest wygrany tylko jeśli WSZYSTKIE typy są wygrane
            bool isLost = false;
            bool allSettled = true;

            foreach (var bet in ticket.Bets)
            {
                var outcomeStatus = await GetOutcomeStatusAsync(bet.OutcomeId);
                
                if (outcomeStatus == OutcomeResult.Lost)
                {
                    isLost = true;
                    break;
                }
                if (outcomeStatus == OutcomeResult.Pending)
                {
                    allSettled = false;
                }
            }

            if (isLost)
            {
                ticket.Settle(TicketStatus.Lost);
                await _ticketRepository.UpdateAsync(ticket);
            }
            else if (allSettled)
            {
                ticket.Settle(TicketStatus.Won);
                await _ticketRepository.UpdateAsync(ticket);
                
                // Automatyczna wypłata wygranej
                await _walletService.ProcessPayoutAsync(ticket.UserId, ticket.PotentialWinning);
            }
        }
    }

    private async Task<OutcomeResult> GetOutcomeStatusAsync(Guid outcomeId)
    {
        // Symulacja sprawdzania statusu wyniku
        return OutcomeResult.Won; // Placeholder
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

public enum OutcomeResult { Pending, Won, Lost }
