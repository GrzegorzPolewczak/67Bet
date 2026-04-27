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

    public Event(Guid sportId, string name, DateTime startTime, string metadata = "{}")
    {
        SportId = sportId;
        Name = name;
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
