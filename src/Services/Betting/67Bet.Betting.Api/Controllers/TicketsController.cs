using Microsoft.AspNetCore.Mvc;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Mappings;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _67Bet.Betting.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IBettingService _bettingService;

    public TicketsController(IBettingService bettingService)
    {
        _bettingService = bettingService;
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> PlaceTicket(PlaceTicketRequest request)
    {
        var userId = GetUserId();
        var ticket = await _bettingService.PlaceTicketAsync(userId, request.Stake, request.OutcomeIds);
        return Ok(ticket.ToDto());
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetMyTickets()
    {
        var userId = GetUserId();
        var tickets = await _bettingService.GetUserTicketsAsync(userId);
        return Ok(tickets.Select(t => t.ToDto()));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }
}
