using _67Bet.Odds.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Odds.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalOddsController : ControllerBase
{
    private readonly IOddsIntegrationService _integrationService;

    public ExternalOddsController(IOddsIntegrationService integrationService)
    {
        _integrationService = integrationService;
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _integrationService.GetEventsAsync();
        return Ok(events);
    }

    [HttpPost("sync")]
    // [Authorize(Roles = "Admin")] // Tymczasowo zakomentowane do testów
    public async Task<IActionResult> SyncOdds()
    {
        var result = await _integrationService.SyncExternalOddsAsync();
        return Ok(result);
    }
}
