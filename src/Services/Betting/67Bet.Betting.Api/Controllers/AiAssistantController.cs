using _67Bet.Betting.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Betting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiAssistantController : ControllerBase
{
    private readonly IAiAssistantService _aiService;

    public AiAssistantController(IAiAssistantService aiService)
    {
        _aiService = aiService;
    }

    [HttpGet("event/{eventId}/insight")]
    public async Task<IActionResult> GetMatchInsight(string eventId)
    {
        try
        {
            var insight = await _aiService.GetMatchInsightAsync(eventId);
            return Ok(new { insight });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Wystąpił nieoczekiwany błąd podczas generowania analizy AI.", details = ex.Message });
        }
    }

    [HttpGet("admin/insights")]
    // [Authorize(Roles = "Admin")] // Odkomentuj gdy auth będzie w pełni gotowe na Azure
    public async Task<IActionResult> GetAllInsights()
    {
        var insights = await _aiService.GetAllInsightsAsync();
        return Ok(insights);
    }

    [HttpPost("admin/event/{eventId}/regenerate")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegenerateInsight(string eventId)
    {
        try
        {
            var insight = await _aiService.RegenerateInsightAsync(eventId);
            return Ok(new { insight });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Błąd podczas regeneracji analizy.", details = ex.Message });
        }
    }

    [HttpDelete("admin/event/{eventId}")]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInsight(string eventId)
    {
        var success = await _aiService.DeleteInsightAsync(eventId);
        if (success) return NoContent();
        return BadRequest(new { message = "Nie udało się usunąć analizy." });
    }
}
