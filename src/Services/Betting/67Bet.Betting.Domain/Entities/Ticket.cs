using System;
using System.Collections.Generic;
using System.Linq;
using _67Bet.Shared.Kernel;
using _67Bet.Betting.Domain.Enums;

namespace _67Bet.Betting.Domain.Entities;

public class Ticket : BaseEntity, IAggregateRoot
{
    public Guid UserId { get; private set; }
    public decimal TotalOdds { get; private set; }
    public decimal Stake { get; private set; }
    public decimal PotentialWinning { get; private set; }
    public TicketStatus Status { get; private set; }
    public List<Bet> Bets { get; private set; } = new();

    public Ticket(Guid userId, decimal stake)
    {
        UserId = userId;
        Stake = stake;
        Status = TicketStatus.Pending;
        TotalOdds = 1.0m;
    }

    public void AddBet(Guid outcomeId, decimal fixedPrice)
    {
        if (Status != TicketStatus.Pending)
            throw new InvalidOperationException("Cannot add bets to a non-pending ticket.");

        var bet = new Bet(Id, outcomeId, fixedPrice);
        Bets.Add(bet);
        CalculateTotalOdds();
    }

    private void CalculateTotalOdds()
    {
        TotalOdds = Bets.Aggregate(1.0m, (acc, bet) => acc * bet.FixedPrice);
        PotentialWinning = Stake * TotalOdds;
    }

    public void Settle(TicketStatus status)
    {
        Status = status;
    }

    // EF Core
    private Ticket() { }
}

public class Bet : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid OutcomeId { get; private set; }
    public decimal FixedPrice { get; private set; }
    public BetStatus Status { get; private set; }

    public Bet(Guid ticketId, Guid outcomeId, decimal fixedPrice)
    {
        TicketId = ticketId;
        OutcomeId = outcomeId;
        FixedPrice = fixedPrice;
        Status = BetStatus.Pending;
    }

    public void Settle(BetStatus status)
    {
        Status = status;
    }

    // EF Core
    private Bet() { }
}
