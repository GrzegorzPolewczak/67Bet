using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Application.Interfaces;

public interface ILiveDataProvider
{
    Task<LiveMatchStateDto?> GetLiveMatchStateAsync(string matchId, string sportKey, string homeTeam, string awayTeam);
}
