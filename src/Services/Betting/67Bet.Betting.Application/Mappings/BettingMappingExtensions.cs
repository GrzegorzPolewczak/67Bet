using System;
using System.Linq;
using System.Text.Json;
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
            @event.League,
            GetSportKey(@event),
            GetSource(@event),
            markets.Select(m => m.ToDto()).ToList()
        );
    }

    private static string GetSportKey(Event @event)
    {
        if (string.IsNullOrWhiteSpace(@event.Metadata))
            return @event.League;

        try
        {
            using var document = JsonDocument.Parse(@event.Metadata);
            if (document.RootElement.TryGetProperty("sportKey", out var sportKeyElement))
            {
                var sportKey = sportKeyElement.GetString();
                if (!string.IsNullOrWhiteSpace(sportKey))
                    return sportKey;
            }
        }
        catch
        {
            // Metadata is optional and should never block event rendering.
        }

        return @event.League;
    }

    private static string GetSource(Event @event)
    {
        if (string.IsNullOrWhiteSpace(@event.Metadata))
            return "internal";

        try
        {
            using var document = JsonDocument.Parse(@event.Metadata);
            if (document.RootElement.TryGetProperty("source", out var sourceElement))
            {
                var source = sourceElement.GetString();
                if (string.Equals(source, "external", StringComparison.OrdinalIgnoreCase))
                    return "external";
            }
        }
        catch
        {
            // Metadata is optional and should never block event rendering.
        }

        return "internal";
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
            ticket.CreatedAt,
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
