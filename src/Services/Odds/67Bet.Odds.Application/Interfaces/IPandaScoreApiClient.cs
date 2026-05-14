using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Application.Interfaces;

public interface IPandaScoreApiClient
{
    Task<IEnumerable<ExternalEventDto>> GetUpcomingEsportsMatchesAsync();
}
