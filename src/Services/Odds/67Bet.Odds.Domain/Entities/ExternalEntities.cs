using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Odds.Domain.Entities;

public class ExternalEvent : BaseEntity, IAggregateRoot
{
    public string ExternalId { get; private set; }
    public string SportKey { get; private set; }
    public string Name { get; private set; }
    public DateTime StartTime { get; private set; }
    private readonly List<ExternalMarket> _markets = new();
    public IReadOnlyCollection<ExternalMarket> Markets => _markets.AsReadOnly();

    public ExternalEvent(string externalId, string sportKey, string name, DateTime startTime)
    {
        ExternalId = externalId;
        SportKey = sportKey;
        Name = name;
        StartTime = startTime;
    }

    public void AddMarket(ExternalMarket market)
    {
        if (!_markets.Any(m => m.Name == market.Name))
        {
            _markets.Add(market);
        }
    }

    public void UpdateInfo(string name, DateTime startTime)
    {
        Name = name;
        StartTime = startTime;
    }
}

public class ExternalMarket : BaseEntity
{
    public Guid ExternalEventId { get; private set; }
    public string Name { get; private set; }
    private readonly List<ExternalOutcome> _outcomes = new();
    public IReadOnlyCollection<ExternalOutcome> Outcomes => _outcomes.AsReadOnly();

    public ExternalMarket(Guid externalEventId, string name)
    {
        ExternalEventId = externalEventId;
        Name = name;
    }

    public void AddOutcome(ExternalOutcome outcome)
    {
        var existing = _outcomes.FirstOrDefault(o => o.Name == outcome.Name);
        if (existing != null)
        {
            existing.UpdatePrice(outcome.Price);
        }
        else
        {
            _outcomes.Add(outcome);
        }
    }
}

public class ExternalOutcome : BaseEntity
{
    public Guid ExternalMarketId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public ExternalOutcome(Guid externalMarketId, string name, decimal price)
    {
        ExternalMarketId = externalMarketId;
        Name = name;
        Price = price;
    }

    public void UpdatePrice(decimal price)
    {
        Price = price;
    }
}
