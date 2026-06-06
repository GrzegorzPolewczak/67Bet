using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Application.Interfaces;

public interface ITheOddsApiClient
{
    Task<IEnumerable<ExternalEventDto>> GetUpcomingEventsAsync(string sport = "soccer_poland_ekstraklasa", string regions = "eu", string markets = "h2h");
    Task<string> GetScoresRawAsync(string sport, int daysFrom = 10);
}

public interface IOddsIntegrationService
{
    Task<SyncResult> SyncExternalOddsAsync();
    Task<IEnumerable<ExternalEventDto>> GetEventsAsync();
}
