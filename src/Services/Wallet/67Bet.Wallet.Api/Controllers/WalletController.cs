using Microsoft.AspNetCore.Mvc;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _67Bet.Wallet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance()
    {
        var userId = GetUserId();
        var balance = await _walletService.GetBalanceAsync(userId);
        var wallet = await _walletService.GetWalletByUserIdAsync(userId);
        
        return Ok(new WalletBalanceDto(balance, wallet?.Currency ?? "PLN"));
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(DepositRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi być dodatnia.");
        
        await _walletService.DepositAsync(GetUserId(), request.Amount);
        return NoContent();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(WithdrawRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi być dodatnia.");
        
        try
        {
            await _walletService.WithdrawAsync(GetUserId(), request.Amount);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }
}
