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
}
