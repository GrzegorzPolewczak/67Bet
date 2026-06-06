using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities;

public class AiMatchInsight : BaseEntity, IAggregateRoot
{
    public string EventId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime GeneratedAt { get; private set; }

    // EF Core
    private AiMatchInsight() { }

    public AiMatchInsight(string eventId, string content)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        EventId = eventId;
        Content = content;
        GeneratedAt = DateTime.UtcNow;
    }

    public void UpdateInsight(string newContent)
    {
        Content = newContent;
        GeneratedAt = DateTime.UtcNow;
    }
}
