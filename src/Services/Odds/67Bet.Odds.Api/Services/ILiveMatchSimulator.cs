using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public interface ILiveMatchSimulator
{
    string SportKey { get; }
    void Update(LiveMatchStateDto match);
}
