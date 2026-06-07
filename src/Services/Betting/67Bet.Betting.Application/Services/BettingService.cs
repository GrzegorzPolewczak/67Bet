using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities;
using _67Bet.Betting.Domain.Repositories;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.VirtualRacing;

namespace _67Bet.Betting.Application.Services;

public class BettingService : IBettingService
{
    private readonly IEventRepository _eventRepository;
    private readonly IMarketRepository _marketRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IWalletService _walletService;
    private readonly IVirtualRaceRepository _virtualRaceRepository;
    private readonly IGamificationService _gamificationService;

    public BettingService(
        IEventRepository eventRepository,
        IMarketRepository marketRepository,
        ITicketRepository ticketRepository,
        IWalletService walletService,
        IVirtualRaceRepository virtualRaceRepository,
        IGamificationService gamificationService)
    {
        _eventRepository = eventRepository;
        _marketRepository = marketRepository;
        _ticketRepository = ticketRepository;
        _walletService = walletService;
        _virtualRaceRepository = virtualRaceRepository;
        _gamificationService = gamificationService;
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

        var stakeProcessed = await _walletService.ProcessStakeAsync(userId, stake);
        if (!stakeProcessed)
            throw new InvalidOperationException("Niewystarczające środki na koncie użytkownika.");

        var ticket = new Ticket(userId, stake);
        var activeEvents = await _eventRepository.GetActiveEventsAsync();
        var activeVirtualRaces = await _virtualRaceRepository.GetActiveRacesAsync();

        foreach (var outcomeId in outcomeIds)
        {
            bool found = false;

            // Search in regular events
            foreach (var @event in activeEvents)
                var events = await _eventRepository.GetActiveEventsAsync();
            Outcome? foundOutcome = null;

            foreach (var @event in events)
            {
                var market = @event.Markets.FirstOrDefault(m => m.Outcomes.Any(o => o.Id == outcomeId));
                if (market != null && market.IsActive)
                {
                    var outcome = market.Outcomes.First(o => o.Id == outcomeId);
                    ticket.AddBet(outcome.Id, outcome.Name, market.Name, @event.Name, @event.StartTime, outcome.CurrentPrice);
                    found = true;
                    break;
                    if (!market.IsActive) continue;

                    var outcome = market.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
                    if (outcome != null)
                    {
                        foundOutcome = outcome;
                        break;
                    }
                }
            }

            if (found) continue;

            // Search in virtual races
            foreach (var race in activeVirtualRaces)
            {
                var participant = race.Participants.FirstOrDefault(p => p.Id == outcomeId);
                if (participant != null)
                    var virtualRaces = await _virtualRaceRepository.GetActiveRacesAsync();
                VirtualRaceParticipant? foundVirtualParticipant = null;

                foreach (var race in virtualRaces)
                {
                    ticket.AddBet(participant.Id, participant.Horse.Name, "Winner", race.Name, race.StartTime, participant.Odds);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                await _walletService.ProcessPayoutAsync(userId, stake);
                throw new InvalidOperationException($"Nie znaleziono aktywnego wyniku lub uczestnika wirtualnego wyścigu o ID: {outcomeId}");
            }
        }

        await _ticketRepository.AddAsync(ticket);

        // Award XP for placing a bet
        await _gamificationService.AwardXpForBetAsync(userId, stake);

        return ticket;
    }

    public async Task SettleEventAsync(Guid eventId, List<Guid> winningOutcomeIds)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) throw new InvalidOperationException("Wydarzenie nie istnieje.");

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

        var tickets = await _ticketRepository.GetActiveTicketsAsync();

        foreach (var ticket in tickets)
        {
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

            bool isLost = false;
            bool allSettled = true;

            foreach (var bet in ticket.Bets)
            {
                var outcomeStatus = await GetOutcomeStatusAsync(bet.OutcomeId);

                if (outcomeStatus == OutcomeResult.Lost)
                {
                    bet.Settle(BetStatus.Won);
                }
                else if (outcomeStatus == OutcomeResult.Lost)
                {
                    bet.Settle(BetStatus.Lost);
                    isLost = true;
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

                await _walletService.ProcessPayoutAsync(ticket.UserId, ticket.PotentialWinning);

                // Award XP for winning a bet
                await _gamificationService.AwardXpForWinAsync(ticket.UserId, ticket.Stake, ticket.TotalOdds);
            }
            else
            {
                // Partially settled, still update to save bet statuses
                await _ticketRepository.UpdateAsync(ticket);
            }
        }
    }

    private async Task<OutcomeResult> GetOutcomeStatusAsync(Guid outcomeId)
    {
        // Ta metoda powinna sprawdzać status konkretnego wyniku w bazie
        // Dla uproszczenia w tej wersji zwracamy status z encji Outcome
        var markets = await _marketRepository.GetAllAsync(); // To jest mało wydajne, ale w tej skali akceptowalne
        foreach (var m in markets)
        {
            var outcome = m.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
            if (outcome != null)
            {
                if (outcome.IsWinner == true) return OutcomeResult.Won;
                if (outcome.IsWinner == false) return OutcomeResult.Lost;
                return OutcomeResult.Pending;
            }
        }

        // Sprawdź wirtualne wyścigi
        var races = await _virtualRaceRepository.GetActiveRacesAsync();
        foreach (var race in races)
        {
            var p = race.Participants.FirstOrDefault(p => p.Id == outcomeId);
            if (p != null)
            {
                if (race.IsFinished)
                {
                    return race.WinningHorseId == p.HorseId ? OutcomeResult.Won : OutcomeResult.Lost;
                }
                return OutcomeResult.Pending;
            }
        }

        return OutcomeResult.Pending;
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
