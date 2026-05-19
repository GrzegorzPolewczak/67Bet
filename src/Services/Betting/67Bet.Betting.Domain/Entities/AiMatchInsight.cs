using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities;

public class AiMatchInsight : BaseEntity, IAggregateRoot
{
    public Guid EventId { get; private set; }
    public string Content { get; private set; } = string.Empty;

    // EF Core
    private AiMatchInsight() { }

    public AiMatchInsight(Guid eventId, string content)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        EventId = eventId;
        Content = content;
    }
}
