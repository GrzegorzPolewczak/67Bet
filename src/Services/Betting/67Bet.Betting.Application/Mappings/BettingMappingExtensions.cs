using System.Linq;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Domain.Entities;

namespace _67Bet.Betting.Application.Mappings;

public static class BettingMappingExtensions
{
    public static EventDto ToDto(this Event @event, IEnumerable<Market> markets)
    {
        return new EventDto(
            @event.Id,
            @event.Name,
            @event.StartTime,
            @event.Status.ToString(),
            markets.Select(m => m.ToDto()).ToList()
        );
    }

    public static MarketDto ToDto(this Market market)
    {
        return new MarketDto(
            market.Id,
            market.Name,
            market.Outcomes.Select(o => o.ToDto()).ToList()
        );
    }

    public static OutcomeDto ToDto(this Outcome outcome)
    {
        return new OutcomeDto(
            outcome.Id,
            outcome.Name,
            outcome.CurrentPrice
        );
    }

    public static TicketDto ToDto(this Ticket ticket)
    {
        return new TicketDto(
            ticket.Id,
            ticket.Stake,
            ticket.TotalOdds,
            ticket.PotentialWinning,
            ticket.Status.ToString(),
            ticket.Bets.Select(b => b.ToDto()).ToList()
        );
    }

    public static BetDto ToDto(this Bet bet)
    {
        var resultStatus = bet.Status.ToString();

        return new BetDto(
            bet.OutcomeId,
            bet.OutcomeName,
            bet.MarketName,
            bet.EventName,
            bet.StartTime,
            bet.FixedPrice,
            bet.Status.ToString(),
            resultStatus,
            bet.WinningOutcomeName
        );
    }
}
