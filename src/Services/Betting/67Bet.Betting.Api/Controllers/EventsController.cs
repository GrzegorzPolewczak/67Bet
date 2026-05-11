using Microsoft.AspNetCore.Mvc;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Mappings;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace _67Bet.Betting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IBettingService _bettingService;

    public Func<Guid, Task<IEnumerable<_67Bet.Betting.Domain.Entities.Market>>>? GetMarketsDelegate { get; set; }

    public EventsController(IBettingService bettingService)
    {
        _bettingService = bettingService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetActiveEvents()
    {
        var events = await _bettingService.GetActiveEventsAsync();
        var dtos = events.Select(e => e.ToDto(e.Markets.ToList())).ToList();
        return Ok(dtos);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/settle")]
    public async Task<IActionResult> SettleEvent(Guid id, SettleEventRequest request)
    {
        await _bettingService.SettleEventAsync(id, request.WinningOutcomeIds);
        return NoContent();
    }
}
