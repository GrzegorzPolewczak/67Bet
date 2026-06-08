using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace _67Bet.Betting.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class RouletteController : ControllerBase
{
    private const string LocalWalletBaseUrl = "http://localhost:5200/api/";
    private const string ProductionWalletBaseUrl = "https://67bet-wallet-api-h9f5epe3heb2dwe0.swedencentral-01.azurewebsites.net/api/";

    private readonly IRouletteService _rouletteService;

    public RouletteController(IResponsibleGamblingService responsibleGamblingService, IRouletteRoundRepository roundRepository)
    {
        var walletBaseUrl = Environment.GetEnvironmentVariable("ROULETTE_WALLET_API_URL") ?? GetDefaultWalletBaseUrl();
        _rouletteService = new RouletteService(roundRepository, new HttpRouletteWalletGateway(walletBaseUrl), responsibleGamblingService);
    }

    [HttpPost("play")]
    public async Task<ActionResult<RouletteRoundDto>> Play(RoulettePlayRequest request)
    {
        try
        {
            var result = await _rouletteService.PlayAsync(GetUserId(), request, GetBearerToken());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("{roundId:guid}/settle")]
    public async Task<ActionResult<RouletteRoundDto>> Settle(Guid roundId)
    {
        try
        {
            var result = await _rouletteService.SettleRoundAsync(GetUserId(), roundId, GetBearerToken());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyCollection<RouletteRoundDto>>> GetHistory([FromQuery] int limit = 10)
    {
        var rounds = await _rouletteService.GetHistoryAsync(GetUserId(), limit);
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

    private sealed class HttpRouletteWalletGateway : IRouletteWalletGateway
    {
        private readonly string _walletBaseUrl;

        public HttpRouletteWalletGateway(string walletBaseUrl)
        {
            _walletBaseUrl = walletBaseUrl.EndsWith('/') ? walletBaseUrl : walletBaseUrl + "/";
        }

        public async Task<bool> ProcessStakeAsync(Guid userId, decimal amount, string? bearerToken)
        {
            using var httpClient = CreateClient(bearerToken);
            var response = await httpClient.PostAsJsonAsync("RouletteWallet/process-stake", new WalletAmountRequest(amount));
            return response.IsSuccessStatusCode;
        }

        public async Task ProcessPayoutAsync(Guid userId, decimal amount, string? bearerToken)
        {
            using var httpClient = CreateClient(bearerToken);
            var response = await httpClient.PostAsJsonAsync("RouletteWallet/process-payout", new WalletAmountRequest(amount));
            response.EnsureSuccessStatusCode();
        }

        private HttpClient CreateClient(string? bearerToken)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(_walletBaseUrl) };
            if (!string.IsNullOrWhiteSpace(bearerToken))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return httpClient;
        }

        private sealed record WalletAmountRequest(decimal Amount);
    }
}
