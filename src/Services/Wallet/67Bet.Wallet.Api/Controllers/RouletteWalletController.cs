using System.Security.Claims;
using _67Bet.Wallet.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Wallet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class RouletteWalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public RouletteWalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost("process-stake")]
    public async Task<IActionResult> ProcessStake(WalletAmountRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Amount must be greater than zero.");

        var accepted = await _walletService.ProcessStakeAsync(GetUserId(), request.Amount);
        if (!accepted) return BadRequest("Insufficient wallet balance.");

        return NoContent();
    }

    [HttpPost("process-payout")]
    public async Task<IActionResult> ProcessPayout(WalletAmountRequest request)
    {
        if (request.Amount < 0) return BadRequest("Amount cannot be negative.");
        if (request.Amount == 0) return NoContent();

        await _walletService.ProcessPayoutAsync(GetUserId(), request.Amount);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }

    public sealed record WalletAmountRequest(decimal Amount);
}
