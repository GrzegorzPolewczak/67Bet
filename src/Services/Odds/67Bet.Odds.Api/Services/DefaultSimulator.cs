using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public class DefaultSimulator : BaseMatchSimulator
{
    public override string SportKey => "default";

    protected override void Simulate(LiveMatchStateDto match)
    {
        match.CurrentAction = "In Progress";
    }
}
