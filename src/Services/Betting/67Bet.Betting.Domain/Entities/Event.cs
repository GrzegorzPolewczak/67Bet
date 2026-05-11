using System;
using System.Collections.Generic;
using _67Bet.Shared.Kernel;
using _67Bet.Betting.Domain.Enums;

namespace _67Bet.Betting.Domain.Entities;

public class Sport : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;

    public Sport(string name)
    {
        Name = name;
    }

    // EF Core
    private Sport() { }
}

public class Event : BaseEntity, IAggregateRoot
{
    public Guid SportId { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public EventStatus Status { get; private set; }
    public string Metadata { get; private set; } = null!; // JSONB in DB
    public string League { get; private set; } = null!;

    public virtual ICollection<Market> Markets { get; private set; } = new List<Market>();

    public Event(string name, Guid sportId, string league, DateTime startTime, string metadata = "{}")
    {
        Name = name;
        SportId = sportId;
        League = league;
        StartTime = startTime;
        Status = EventStatus.Scheduled;
        Metadata = metadata;
    }

    public void UpdateStatus(EventStatus status)
    {
        Status = status;
    }

    // EF Core
    private Event() { }
}
