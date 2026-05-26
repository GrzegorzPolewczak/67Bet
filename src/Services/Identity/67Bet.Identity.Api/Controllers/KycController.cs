using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using _67Bet.Identity.Api.Hubs;
using _67Bet.Identity.Application.Interfaces;

namespace _67Bet.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/kyc")]
    public class KycController : ControllerBase
    {
        private readonly IKycService _kycService;
        private readonly IHubContext<VerificationHub> _hubContext;

        public KycController(IKycService kycService, IHubContext<VerificationHub> hubContext)
        {
            _kycService = kycService;
            _hubContext = hubContext;
        }

        [HttpGet("session")]
        [Authorize]
        public async Task<IActionResult> GetSession()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var sessionId = await _kycService.GenerateSessionAsync(userId);
            return Ok(new { sessionId });
        }

        [HttpPost("verify/{sessionId}")]
        public async Task<IActionResult> Verify(Guid sessionId, IFormFile idCard, IFormFile selfie)
        {
            if (idCard == null || selfie == null)
            {
                return BadRequest("Both idCard and selfie are required.");
            }

            // Simulate AI verification process
            await Task.Delay(3000);

            // Update session status in database
            await _kycService.CompleteSessionAsync(sessionId);

            // Broadcast completion event to the specific session group
            await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("VerificationCompleted");

            return Ok();
        }
    }
}
