using System.Security.Claims;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Betting.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/responsible-gambling")]
public sealed class ResponsibleGamblingController : ControllerBase
{
    private readonly IResponsibleGamblingService _responsibleGamblingService;

    public ResponsibleGamblingController(IResponsibleGamblingService responsibleGamblingService)
    {
        _responsibleGamblingService = responsibleGamblingService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ResponsibleGamblingDashboardDto>> GetMyDashboard()
    {
        return Ok(await _responsibleGamblingService.GetDashboardAsync(GetUserId()));
    }

    [HttpPost("me/limits")]
    public async Task<ActionResult<ResponsibleGamblingLimitDto>> SetLimit(SetResponsibleGamblingLimitRequest request)
    {
        return Ok(await _responsibleGamblingService.SetLimitAsync(GetUserId(), request));
    }

    [HttpPost("me/self-exclusion")]
    public async Task<ActionResult<SelfExclusionDto>> StartSelfExclusion(StartSelfExclusionRequest request)
    {
        return Ok(await _responsibleGamblingService.StartSelfExclusionAsync(GetUserId(), request));
    }

    [HttpPost("me/validate-stake")]
    public async Task<ActionResult<ResponsibleGamblingValidationResultDto>> ValidateStake(ResponsibleGamblingValidationRequest request)
    {
        return Ok(await _responsibleGamblingService.ValidateStakeAsync(GetUserId(), request.Amount));
    }

    [HttpPost("me/validate-deposit")]
    public async Task<ActionResult<ResponsibleGamblingValidationResultDto>> ValidateDeposit(ResponsibleGamblingValidationRequest request)
    {
        return Ok(await _responsibleGamblingService.ValidateDepositAsync(GetUserId(), request.Amount));
    }

    [HttpPost("me/activity")]
    public async Task<IActionResult> RecordActivity(RecordResponsibleGamblingActivityRequest request)
    {
        await _responsibleGamblingService.RecordActivityAsync(GetUserId(), request);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null) throw new UnauthorizedAccessException("User ID not found in token.");
        return Guid.Parse(userIdClaim.Value);
    }
}
