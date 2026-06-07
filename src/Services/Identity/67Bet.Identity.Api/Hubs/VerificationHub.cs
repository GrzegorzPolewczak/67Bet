using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace _67Bet.Identity.Api.Hubs
{
    public class VerificationHub : Hub
    {
        public async Task JoinGroup(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        }
    }
}
