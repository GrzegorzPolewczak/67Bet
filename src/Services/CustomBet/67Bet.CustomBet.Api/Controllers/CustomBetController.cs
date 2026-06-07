using Microsoft.AspNetCore.Mvc;
using _67Bet.CustomBet.Application.Interfaces;
using _67Bet.CustomBet.Application.DTOs;
using _67Bet.CustomBet.Application.Mappings;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _67Bet.CustomBet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomBetController : ControllerBase
{
    private readonly ICustomBetService _customBetService;

    public CustomBetController(ICustomBetService customBetService)
    {
        _customBetService = customBetService;
    }

    [Authorize]
    [HttpPost("requests")]
    public async Task<IActionResult> SubmitRequest([FromBody] System.Text.Json.JsonElement body)
    {
        try
        {
            if (!body.TryGetProperty("description", out var descriptionProp))
            {
                return BadRequest(new { message = "Pole 'description' jest wymagane w JSON." });
            }

            var description = descriptionProp.GetString();
            if (string.IsNullOrEmpty(description))
            {
                return BadRequest(new { message = "Opis nie może być pusty." });
            }

            var userId = GetUserId();
            var result = await _customBetService.CreateRequestAsync(userId, description);
            return Ok(result.ToDto());
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                message = "Błąd podczas tworzenia wniosku Custom Bet", 
                details = ex.Message,
                innerDetails = ex.InnerException?.Message,
                stackTrace = ex.StackTrace 
            });
        }
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<CustomBetRequestDto>>> GetMyRequests()
    {
        var userId = GetUserId();
        var requests = await _customBetService.GetUserRequestsAsync(userId);
        return Ok(requests.Select(r => r.ToDto()));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<CustomBetRequestDto>>> GetPendingRequests()
    {
        var requests = await _customBetService.GetPendingRequestsAsync();
        return Ok(requests.Select(r => r.ToDto()));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("requests/{id}/accept")]
    public async Task<IActionResult> AcceptRequest(Guid id, AcceptCustomBetRequest request)
    {
        await _customBetService.AcceptRequestAsync(id, request.FinalOdds, request.AdminNote);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("requests/{id}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, RejectCustomBetRequest request)
    {
        await _customBetService.RejectRequestAsync(id, request.Reason);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("requests/{id}/recommendation")]
    public async Task<ActionResult<CustomBetRequestDto>> GetAiRecommendation(Guid id)
    {
        var result = await _customBetService.GetAiRecommendationAsync(id);
        return Ok(result.ToDto());
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }
}
