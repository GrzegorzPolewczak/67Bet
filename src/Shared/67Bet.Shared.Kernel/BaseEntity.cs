/*
 * Klasa bazowa dla wszystkich encji w systemie.
 * Zapewnia unikalny identyfikator Guid oraz interfejsy znacznikowe dla DDD.
 */
using System;

namespace _67Bet.Shared.Kernel;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}

public interface IAggregateRoot { }

