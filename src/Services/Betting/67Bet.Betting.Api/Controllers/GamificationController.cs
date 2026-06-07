using _67Bet.Betting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _67Bet.Betting.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GamificationController : ControllerBase
{
    private readonly IGamificationService _gamificationService;

    public GamificationController(IGamificationService gamificationService)
    {
        _gamificationService = gamificationService;
    }

    [HttpGet("me/progress")]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = GetUserId();
        var progress = await _gamificationService.GetUserProgressAsync(userId);
        return Ok(progress);
    }

    [HttpGet("me/achievements")]
    public async Task<IActionResult> GetMyAchievements()
    {
        var userId = GetUserId();
        var achievements = await _gamificationService.GetUserAchievementsAsync(userId);
        return Ok(achievements);
    }

    [HttpPost("me/daily-login")]
    public async Task<IActionResult> ProcessDailyLogin()
    {
        var userId = GetUserId();
        await _gamificationService.ProcessDailyLoginAsync(userId);
        return Ok(new { message = "Daily login processed successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("admin/award-xp")]
    public async Task<IActionResult> AwardXp(Guid userId, long amount)
    {
        // Simple manual award for admins
        // This could be expanded in IGamificationService if needed
        return Ok(new { message = "Manual XP award is not yet fully implemented for admins, but the request was received." });
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User ID not found in token.");

        return Guid.Parse(userIdClaim.Value);
    }
}
