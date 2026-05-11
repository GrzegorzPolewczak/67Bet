using Microsoft.AspNetCore.Mvc;
using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace _67Bet.Odds.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class OddsController : ControllerBase
{
    private readonly IOddsService _oddsService;

    public OddsController(IOddsService oddsService)
    {
        _oddsService = oddsService;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<OddsResultDto>> CalculateOdds(CalculateOddsRequest request)
    {
        var odds = await _oddsService.CalculateOddsAsync(request.Probability);
        return Ok(new OddsResultDto(odds, request.Probability));
    }

    [HttpPost("update/{marketId}")]
    public async Task<IActionResult> UpdateMarketOdds(Guid marketId)
    {
        await _oddsService.UpdateMarketOddsAsync(marketId);
        return NoContent();
    }

    [HttpPost("probability")]
    public async Task<ActionResult<decimal>> GetLiveProbability(LiveProbabilityRequest request)
    {
        var probability = await _oddsService.GetLiveProbabilityAsync(request.EventId, request.ContextJson);
        return Ok(probability);
    }
}
