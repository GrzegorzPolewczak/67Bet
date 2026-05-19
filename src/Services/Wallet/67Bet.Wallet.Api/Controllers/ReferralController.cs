using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.DTOs;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace _67Bet.Wallet.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReferralController : ControllerBase
    {
        private readonly IReferralService _referralService;

        public ReferralController(IReferralService referralService)
        {
            _referralService = referralService;
        }

        [HttpGet("status")]
        public async Task<ActionResult<ReferralStatusDto>> GetStatus()
        {
            var userId = GetUserId();
            var status = await _referralService.GetReferralStatusAsync(userId);
            return Ok(status);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCode([FromBody] string code)
        {
            var userId = GetUserId();
            try
            {
                await _referralService.CreateCreatorCodeAsync(userId, code);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyCode([FromBody] string code)
        {
            var userId = GetUserId();
            try
            {
                await _referralService.ApplyCodeAsync(userId, code);
                return Ok(new { message = "Kod zaakceptowany! Bonus Freebet dodany do konta." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/promo")]
        public async Task<ActionResult<IEnumerable<PromoCodeDto>>> GetAllPromoCodes()
        {
            var codes = await _referralService.GetAllPromoCodesAsync();
            return Ok(codes);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/promo")]
        public async Task<IActionResult> CreatePromoCode([FromBody] CreatePromoRequest request)
        {
            try
            {
                await _referralService.CreatePromoCodeAsync(request.Code, request.Reward);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/promo/deactivate")]
        public async Task<IActionResult> DeactivatePromoCode([FromBody] string code)
        {
            try
            {
                await _referralService.DeactivatePromoCodeAsync(code);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/promo/activate")]
        public async Task<IActionResult> ActivatePromoCode([FromBody] string code)
        {
            try
            {
                await _referralService.ActivatePromoCodeAsync(code);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException();
            return Guid.Parse(userIdClaim);
        }
    }

    public class CreatePromoRequest
    {
        public string Code { get; set; } = null!;
        public decimal Reward { get; set; }
    }
}
