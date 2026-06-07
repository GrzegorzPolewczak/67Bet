using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.Plinko;
using _67Bet.Betting.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Betting.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class PlinkoController : ControllerBase
{
    private const string LocalWalletBaseUrl = "http://localhost:5200/api/";
    private const string ProductionWalletBaseUrl = "https://67bet-wallet-api-h9f5epe3heb2dwe0.swedencentral-01.azurewebsites.net/api/";

    private static readonly InMemoryPlinkoRoundRepository RoundRepository = new();

    private readonly IPlinkoService _plinkoService;

    public PlinkoController(IResponsibleGamblingService responsibleGamblingService)
    {
        var walletBaseUrl = Environment.GetEnvironmentVariable("PLINKO_WALLET_API_URL") ?? GetDefaultWalletBaseUrl();
        _plinkoService = new PlinkoService(RoundRepository, new HttpPlinkoWalletGateway(walletBaseUrl), responsibleGamblingService);
    }

    [HttpGet("board")]
    public ActionResult<PlinkoBoardDto> GetBoard([FromQuery] PlinkoRiskLevel riskLevel = PlinkoRiskLevel.Medium, [FromQuery] int rows = 12)
    {
        return Ok(_plinkoService.GetBoard(riskLevel, rows));
    }

    [HttpPost("play")]
    public async Task<ActionResult<PlinkoRoundDto>> Play(PlinkoPlayRequest request)
    {
        var result = await _plinkoService.PlayAsync(GetUserId(), request, GetBearerToken());
        return Ok(result);
    }

    [HttpPost("{roundId:guid}/settle")]
    public async Task<ActionResult<PlinkoRoundDto>> Settle(Guid roundId)
    {
        var result = await _plinkoService.SettleRoundAsync(GetUserId(), roundId, GetBearerToken());
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyCollection<PlinkoRoundDto>>> GetHistory([FromQuery] int limit = 10)
    {
        var rounds = await _plinkoService.GetHistoryAsync(GetUserId(), limit);
        return Ok(rounds);
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(userIdClaim.Value);
    }

    private string? GetBearerToken()
    {
        var header = Request.Headers.Authorization.ToString();
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..]
            : null;
    }

    private static string GetDefaultWalletBaseUrl()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var isAzureAppService = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME"));

        return isAzureAppService || string.Equals(environmentName, "Production", StringComparison.OrdinalIgnoreCase)
            ? ProductionWalletBaseUrl
            : LocalWalletBaseUrl;
    }

    private sealed class HttpPlinkoWalletGateway : IPlinkoWalletGateway
    {
        private readonly string _walletBaseUrl;

        public HttpPlinkoWalletGateway(string walletBaseUrl)
        {
            _walletBaseUrl = walletBaseUrl.EndsWith('/') ? walletBaseUrl : walletBaseUrl + "/";
        }

        public async Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken)
        {
            using var httpClient = CreateClient(bearerToken);
            var response = await httpClient.PostAsJsonAsync("PlinkoWallet/process-stake", new WalletAmountRequest(amount));
            return response.IsSuccessStatusCode;
        }

        public async Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken)
        {
            using var httpClient = CreateClient(bearerToken);
            var response = await httpClient.PostAsJsonAsync("PlinkoWallet/process-payout", new WalletAmountRequest(amount));
            response.EnsureSuccessStatusCode();
        }

        private HttpClient CreateClient(string? bearerToken)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(_walletBaseUrl) };
            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            return httpClient;
        }

        private sealed record WalletAmountRequest(decimal Amount);
    }
}
