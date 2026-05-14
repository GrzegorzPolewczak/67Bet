using System;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public abstract class BaseMatchSimulator : ILiveMatchSimulator
{
    public abstract string SportKey { get; }
    protected readonly Random Random = new();

    public virtual void Update(LiveMatchStateDto match)
    {
        UpdateTime(match);
        Simulate(match);
    }

    protected abstract void Simulate(LiveMatchStateDto match);

    protected void UpdateTime(LiveMatchStateDto match)
    {
        var timeParts = match.CurrentTime.Split(':');
        if (timeParts.Length == 2 && int.TryParse(timeParts[0], out int minutes) && int.TryParse(timeParts[1], out int seconds))
        {
            seconds += 5;
            if (seconds >= 60)
            {
                seconds -= 60;
                minutes++;
            }
            match.CurrentTime = $"{minutes:D2}:{seconds:D2}";
        }
    }
}
