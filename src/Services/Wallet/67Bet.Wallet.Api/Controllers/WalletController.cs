using Microsoft.AspNetCore.Mvc;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace _67Bet.Wallet.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/wallet")]
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
    public async Task<ActionResult<WalletBalanceDto>> GetBalance([FromQuery] Guid? userId)
    {
        try
        {
            // Support both authenticated user and explicit userId for service-to-service calls
            var targetUserId = userId ?? GetUserId();

            var wallet = await _walletService.GetWalletByUserIdAsync(targetUserId);
            if (wallet == null)
            {
                return Ok(new WalletBalanceDto(0, 0, "PLN"));
            }

            return Ok(new WalletBalanceDto(wallet.Balance, wallet.FreebetBalance, wallet.Currency ?? "PLN"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "BĹ‚Ä…d podczas pobierania salda.", details = ex.Message });
        }
    }

    [HttpPost("create-payment-intent")]
    public async Task<ActionResult<PaymentIntentResponseDto>> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi byÄ‡ dodatnia.");

        try
        {
            var result = await _paymentService.CreatePaymentIntentAsync(GetUserId(), request.Amount, request.Currency);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "BĹ‚Ä…d procesora pĹ‚atnoĹ›ci.", details = ex.Message });
        }
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
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi byÄ‡ dodatnia.");

        await _walletService.DepositAsync(GetUserId(), request.Amount);
        return NoContent();
    }

    [HttpPost("deposit-freebet")]
    public async Task<IActionResult> DepositFreebet([FromBody] DepositRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi byÄ‡ dodatnia.");

        await _walletService.DepositFreebetAsync(GetUserId(), request.Amount);
        return NoContent();
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        if (request.Amount <= 0) return BadRequest("Kwota musi byÄ‡ dodatnia.");

        var userId = GetUserId();

        try
        {
            // 1. Check local balance first
            var balance = await _walletService.GetBalanceAsync(userId);
            if (balance < request.Amount) return BadRequest("NiewystarczajÄ…ce Ĺ›rodki na koncie.");

            // 2. Trigger Stripe Payout (Sandbox)
            var errorMessage = await _paymentService.CreatePayoutAsync(userId, request.Amount);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return BadRequest($"BĹ‚Ä…d procesora pĹ‚atnoĹ›ci Stripe: {errorMessage}");
            }

            // 3. Update local balance and register transaction
            await _walletService.WithdrawAsync(userId, request.Amount);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "WystÄ…piĹ‚ nieoczekiwany bĹ‚Ä…d podczas wypĹ‚aty.");
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("Brak identyfikatora uĹĽytkownika w tokenie.");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("NieprawidĹ‚owy format identyfikatora uĹĽytkownika.");

        return userId;
    }
}
