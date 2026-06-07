using _67Bet.Betting.Application.Interfaces;

namespace _67Bet.Betting.Application.Services;

public sealed class SystemResponsibleGamblingClock : IResponsibleGamblingClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
