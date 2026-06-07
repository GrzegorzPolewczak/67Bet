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
        private readonly IBettingService _bettingService;

        public VirtualRacingController(
            IVirtualRacingService virtualRacingService,
            IBettingService bettingService)
        {
            _virtualRacingService = virtualRacingService;
            _bettingService = bettingService;
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


        [HttpPost("settle-finished")]
        public async Task<IActionResult> SettleFinishedRaces()
        {
            try
            {
                await _bettingService.SettleFinishedVirtualRacesAsync();
                return Ok(new { Message = "Finished virtual races settled." });
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
                await _bettingService.SettleVirtualRaceAsync(id);
                return Ok(new { WinningHorseId = winningHorseId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
