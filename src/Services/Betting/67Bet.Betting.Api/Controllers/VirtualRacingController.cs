using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using _67Bet.Betting.Application.Interfaces;

namespace _67Bet.Betting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VirtualRacingController : ControllerBase
    {
        private readonly IVirtualRacingService _virtualRacingService;

        public VirtualRacingController(IVirtualRacingService virtualRacingService)
        {
            _virtualRacingService = virtualRacingService;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRaces()
        {
            var races = await _virtualRacingService.GetActiveRacesAsync();
            return Ok(races);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateRace()
        {
            try
            {
                var race = await _virtualRacingService.GenerateRaceAsync();
                return Ok(race);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("{id:guid}/simulate")]
        public async Task<IActionResult> SimulateRace(Guid id)
        {
            try
            {
                var winningHorseId = await _virtualRacingService.SimulateRaceAsync(id);
                return Ok(new { WinningHorseId = winningHorseId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}