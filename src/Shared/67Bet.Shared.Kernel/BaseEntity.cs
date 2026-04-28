using System;

namespace _67Bet.Shared.Kernel;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}

public interface IAggregateRoot { }
