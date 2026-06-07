using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.DTOs;
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
    private readonly IResponsibleGamblingService _responsibleGamblingService;

    public BettingService(
        IEventRepository eventRepository,
        IMarketRepository marketRepository,
        ITicketRepository ticketRepository,
        IWalletService walletService,
        IVirtualRaceRepository virtualRaceRepository,
        IGamificationService gamificationService,
        IResponsibleGamblingService responsibleGamblingService)
    {
        _eventRepository = eventRepository;
        _marketRepository = marketRepository;
        _ticketRepository = ticketRepository;
        _walletService = walletService;
        _virtualRaceRepository = virtualRaceRepository;
        _gamificationService = gamificationService;
        _responsibleGamblingService = responsibleGamblingService;
    }

    public async Task<IEnumerable<Event>> GetActiveEventsAsync()
    {
        return await _eventRepository.GetActiveEventsAsync();
    }

    public async Task<Ticket> PlaceTicketAsync(Guid userId, decimal stake, IEnumerable<Guid> outcomeIds)
    {
        if (stake <= 0)
            throw new ArgumentException("Stawka musi być większa od zera.");

        var requestedOutcomeIds = outcomeIds?.Distinct().ToList() ?? new List<Guid>();

        if (!requestedOutcomeIds.Any())
            throw new ArgumentException("Kupon musi zawierać przynajmniej jeden zakład.");

        var validation = await _responsibleGamblingService.ValidateStakeAsync(userId, stake);
        if (!validation.IsAllowed)
            throw new InvalidOperationException(validation.Message ?? "Stake is blocked by responsible gambling limits.");

        var activeEvents = await _eventRepository.GetActiveEventsAsync();
        var activeVirtualRaces = await _virtualRaceRepository.GetActiveRacesAsync();

        var resolvedBets = new List<ResolvedBet>();

        foreach (var outcomeId in requestedOutcomeIds)
        {
            var regularBet = ResolveRegularEventBet(activeEvents, outcomeId);
            if (regularBet != null)
            {
                resolvedBets.Add(regularBet);
                continue;
            }

            var virtualRaceBet = ResolveVirtualRaceBet(activeVirtualRaces, outcomeId);
            if (virtualRaceBet != null)
            {
                resolvedBets.Add(virtualRaceBet);
                continue;
            }

            throw new InvalidOperationException(
                $"Nie znaleziono aktywnego wyniku lub uczestnika wirtualnego wyścigu o ID: {outcomeId}");
        }

        // Pobieramy środki dopiero po potwierdzeniu, że wszystkie outcomeIds istnieją.
        // Dzięki temu nie zabieramy środków za kupon, którego backend i tak nie może utworzyć.
        var stakeProcessed = await _walletService.ProcessStakeAsync(userId, stake);
        if (!stakeProcessed)
            throw new InvalidOperationException("Niewystarczające środki na koncie użytkownika.");

        await _responsibleGamblingService.RecordActivityAsync(
            userId,
            new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Stake, stake));

        var ticket = new Ticket(userId, stake);

        foreach (var bet in resolvedBets)
        {
            ticket.AddBet(
                bet.OutcomeId,
                bet.OutcomeName,
                bet.MarketName,
                bet.EventName,
                bet.StartTime,
                bet.FixedPrice);
        }

        await _ticketRepository.AddAsync(ticket);

        // Award XP for placing a bet
        await _gamificationService.AwardXpForBetAsync(userId, stake);

        return ticket;
    }

    private static ResolvedBet? ResolveRegularEventBet(IEnumerable<Event> activeEvents, Guid outcomeId)
    {
        foreach (var @event in activeEvents)
        {
            var market = @event.Markets.FirstOrDefault(m =>
                m.IsActive && m.Outcomes.Any(o => o.Id == outcomeId));

            if (market == null)
                continue;

            var outcome = market.Outcomes.First(o => o.Id == outcomeId);

            return new ResolvedBet(
                outcome.Id,
                outcome.Name,
                market.Name,
                @event.Name,
                @event.StartTime,
                outcome.CurrentPrice);
        }

        return null;
    }

    private static ResolvedBet? ResolveVirtualRaceBet(IEnumerable<VirtualRace> activeVirtualRaces, Guid outcomeId)
    {
        foreach (var race in activeVirtualRaces)
        {
            var participant = race.Participants.FirstOrDefault(p => p.Id == outcomeId);
            if (participant == null)
                continue;

            return new ResolvedBet(
                participant.Id,
                participant.Horse?.Name ?? "Unknown Horse",
                "Winner",
                race.Name,
                race.StartTime,
                participant.Odds);
        }

        return null;
    }

    public async Task SettleEventAsync(Guid eventId, List<Guid> winningOutcomeIds)
    {
        var @event = await _eventRepository.GetByIdAsync(eventId);
        if (@event == null) throw new InvalidOperationException("Wydarzenie nie istnieje.");

        @event.UpdateStatus(EventStatus.Finished);
        await _eventRepository.UpdateAsync(@event);

        var markets = (await _marketRepository.GetByEventIdAsync(eventId)).ToList();
        var eventOutcomeIds = markets
            .SelectMany(m => m.Outcomes)
            .Select(o => o.Id)
            .ToHashSet();

        foreach (var market in markets)
        {
            foreach (var outcome in market.Outcomes)
            {
                var isWinner = winningOutcomeIds.Contains(outcome.Id);
                outcome.SetResult(isWinner);
            }

            await _marketRepository.UpdateAsync(market);
        }

        var tickets = await _ticketRepository.GetActiveTicketsAsync();

        foreach (var ticket in tickets)
        {
            var ticketHasThisEvent = ticket.Bets.Any(b => eventOutcomeIds.Contains(b.OutcomeId));
            if (!ticketHasThisEvent) continue;

            foreach (var bet in ticket.Bets)
            {
                var resolution = await GetOutcomeResolutionAsync(bet.OutcomeId);

                if (resolution.Result == OutcomeResult.Won)
                {
                    bet.Settle(BetStatus.Won, resolution.WinningOutcomeName);
                }
                else if (resolution.Result == OutcomeResult.Lost)
                {
                    bet.Settle(BetStatus.Lost, resolution.WinningOutcomeName);
                }
            }

            await ApplyTicketSettlementAsync(ticket);
        }
    }

    public async Task SettleVirtualRaceAsync(Guid raceId)
    {
        var race = await _virtualRaceRepository.GetByIdAsync(raceId);
        if (race == null)
            throw new InvalidOperationException("Virtual race does not exist.");

        if (!race.IsFinished || race.WinningHorseId == null)
            throw new InvalidOperationException("Virtual race is not finished yet.");

        var participantIds = race.Participants.Select(p => p.Id).ToHashSet();
        var winningParticipant = race.Participants.FirstOrDefault(p => p.HorseId == race.WinningHorseId.Value);

        if (winningParticipant == null)
            throw new InvalidOperationException("Winning participant was not found.");

        var winningOutcomeName = winningParticipant.Horse?.Name ?? "Unknown Horse";
        var tickets = await _ticketRepository.GetActiveTicketsAsync();

        foreach (var ticket in tickets)
        {
            var raceBets = ticket.Bets
                .Where(b => participantIds.Contains(b.OutcomeId))
                .ToList();

            if (!raceBets.Any())
                continue;

            foreach (var bet in raceBets)
            {
                var status = bet.OutcomeId == winningParticipant.Id
                    ? BetStatus.Won
                    : BetStatus.Lost;

                bet.Settle(status, winningOutcomeName);
            }

            await ApplyTicketSettlementAsync(ticket);
        }
    }


    public async Task SettleFinishedVirtualRacesAsync()
    {
        var finishedRaces = await _virtualRaceRepository.GetFinishedRacesAsync();

        foreach (var race in finishedRaces)
        {
            await SettleVirtualRaceAsync(race.Id);
        }
    }

    private async Task ApplyTicketSettlementAsync(Ticket ticket)
    {
        if (ticket.Bets.Any(b => b.Status == BetStatus.Lost))
        {
            ticket.Settle(TicketStatus.Lost);
            await _ticketRepository.UpdateAsync(ticket);
            return;
        }

        if (ticket.Bets.All(b => b.Status == BetStatus.Won))
        {
            ticket.Settle(TicketStatus.Won);
            await _ticketRepository.UpdateAsync(ticket);

            await _walletService.ProcessPayoutAsync(ticket.UserId, ticket.PotentialWinning);
            await _responsibleGamblingService.RecordActivityAsync(
                ticket.UserId,
                new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Payout, ticket.PotentialWinning));

            // Award XP for winning a bet
            await _gamificationService.AwardXpForWinAsync(ticket.UserId, ticket.Stake, ticket.TotalOdds);
            return;
        }

        // Some selections are already settled, but the ticket still contains pending selections.
        await _ticketRepository.UpdateAsync(ticket);
    }

    private async Task<OutcomeResolution> GetOutcomeResolutionAsync(Guid outcomeId)
    {
        var markets = await _marketRepository.GetAllAsync();
        foreach (var market in markets)
        {
            var outcome = market.Outcomes.FirstOrDefault(o => o.Id == outcomeId);
            if (outcome == null)
                continue;

            var winningOutcomeName = string.Join(
                ", ",
                market.Outcomes
                    .Where(o => o.IsWinner == true)
                    .Select(o => o.Name));

            if (string.IsNullOrWhiteSpace(winningOutcomeName))
                winningOutcomeName = null;

            if (outcome.IsWinner == true)
                return new OutcomeResolution(OutcomeResult.Won, winningOutcomeName ?? outcome.Name);

            if (outcome.IsWinner == false)
                return new OutcomeResolution(OutcomeResult.Lost, winningOutcomeName);

            return new OutcomeResolution(OutcomeResult.Pending, null);
        }

        var race = await _virtualRaceRepository.GetByParticipantIdAsync(outcomeId);
        if (race != null)
        {
            var participant = race.Participants.FirstOrDefault(p => p.Id == outcomeId);
            if (participant == null)
                return new OutcomeResolution(OutcomeResult.Pending, null);

            if (!race.IsFinished || race.WinningHorseId == null)
                return new OutcomeResolution(OutcomeResult.Pending, null);

            var winningParticipant = race.Participants.FirstOrDefault(p => p.HorseId == race.WinningHorseId.Value);
            var winningOutcomeName = winningParticipant?.Horse?.Name ?? "Unknown Horse";

            return race.WinningHorseId.Value == participant.HorseId
                ? new OutcomeResolution(OutcomeResult.Won, winningOutcomeName)
                : new OutcomeResolution(OutcomeResult.Lost, winningOutcomeName);
        }

        return new OutcomeResolution(OutcomeResult.Pending, null);
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid ticketId)
    {
        return await _ticketRepository.GetByIdAsync(ticketId);
    }

    public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(Guid userId)
    {
        return await _ticketRepository.GetByUserIdAsync(userId);
    }

    private sealed record ResolvedBet(
        Guid OutcomeId,
        string OutcomeName,
        string MarketName,
        string EventName,
        DateTime StartTime,
        decimal FixedPrice);

    private sealed record OutcomeResolution(OutcomeResult Result, string? WinningOutcomeName);
}

public enum OutcomeResult { Pending, Won, Lost }
