using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities;

public class AiGenerationLog : BaseEntity
{
    public string EventId { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    // EF Core
    private AiGenerationLog() { }

    public AiGenerationLog(string eventId, string status, string? errorMessage = null)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        EventId = eventId;
        Status = status;
        ErrorMessage = errorMessage;
    }
}
