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
    private readonly IPaymentService _paymentService;

    public WalletController(IWalletService walletService, IPaymentService paymentService)
    {
        _walletService = walletService;
        _paymentService = paymentService;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<WalletBalanceDto>> GetBalance()
    {
        var userId = GetUserId();
        var balance = await _walletService.GetBalanceAsync(userId);
        var wallet = await _walletService.GetWalletByUserIdAsync(userId);
        
        return Ok(new WalletBalanceDto(balance, wallet?.Currency ?? "PLN"));
    }

    [HttpPost("create-payment-intent")]
    public async Task<ActionResult<PaymentIntentResponseDto>> CreatePaymentIntent(CreatePaymentIntentRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi być dodatnia.");
        
        var result = await _paymentService.CreatePaymentIntentAsync(GetUserId(), request.Amount, request.Currency);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        var result = await _paymentService.HandleWebhookAsync(json, signature!);
        
        if (result) return Ok();
        return BadRequest();
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
        
        var userId = GetUserId();
        
        try
        {
            // 1. Check local balance first
            var balance = await _walletService.GetBalanceAsync(userId);
            if (balance < request.Amount) return BadRequest("Niewystarczające środki na koncie.");

            // 2. Trigger Stripe Payout (Sandbox)
            var errorMessage = await _paymentService.CreatePayoutAsync(userId, request.Amount);
            if (!string.IsNullOrEmpty(errorMessage)) 
            {
                return BadRequest($"Błąd procesora płatności Stripe: {errorMessage}");
            }

            // 3. Update local balance and register transaction
            await _walletService.WithdrawAsync(userId, request.Amount);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Wystąpił nieoczekiwany błąd podczas wypłaty.");
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }
}
