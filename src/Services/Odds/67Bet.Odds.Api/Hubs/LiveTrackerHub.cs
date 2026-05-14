using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Api.Hubs;

public class LiveTrackerHub : Hub
{
    private readonly ILogger<LiveTrackerHub> _logger;

    public LiveTrackerHub(ILogger<LiveTrackerHub> logger)
    {
        _logger = logger;
    }

    public async Task SubscribeToMatch(string matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, matchId);
        _logger.LogInformation("Client {ConnectionId} subscribed to match {MatchId}", Context.ConnectionId, matchId);
    }

    public async Task UnsubscribeFromMatch(string matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, matchId);
        _logger.LogInformation("Client {ConnectionId} unsubscribed from match {MatchId}", Context.ConnectionId, matchId);
    }
}
