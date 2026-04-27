using System;
using System.Collections.Generic;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities;

public class Market : BaseEntity
{
    public Guid EventId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public List<Outcome> Outcomes { get; private set; } = new();

    public Market(Guid eventId, string name)
    {
        EventId = eventId;
        Name = name;
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    // EF Core
    private Market() { }
}

public class Outcome : BaseEntity
{
    public Guid MarketId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Probability { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public bool? IsWinner { get; private set; }

    public Outcome(Guid marketId, string name, decimal probability, decimal currentPrice)
    {
        MarketId = marketId;
        Name = name;
        Probability = probability;
        CurrentPrice = currentPrice;
    }

    public void SetResult(bool isWinner)
    {
        IsWinner = isWinner;
    }

    public void UpdatePrice(decimal newPrice, decimal newProbability)
    {
        CurrentPrice = newPrice;
        Probability = newProbability;
    }

    // EF Core
    private Outcome() { }
}
