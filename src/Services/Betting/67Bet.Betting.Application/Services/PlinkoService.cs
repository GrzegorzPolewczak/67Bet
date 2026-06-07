using System.Collections.Concurrent;
using System.Security.Cryptography;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.Plinko;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Application.Services;

public sealed class PlinkoService : IPlinkoService
{
    private static readonly ConcurrentDictionary<(PlinkoRiskLevel Risk, int Rows), decimal[]> Boards = new();

    private readonly IPlinkoRoundRepository _roundRepository;
    private readonly IPlinkoWalletGateway _walletGateway;
    private readonly IResponsibleGamblingService? _responsibleGamblingService;

    public PlinkoService(
        IPlinkoRoundRepository roundRepository,
        IPlinkoWalletGateway walletGateway,
        IResponsibleGamblingService? responsibleGamblingService = null)
    {
        _roundRepository = roundRepository;
        _walletGateway = walletGateway;
        _responsibleGamblingService = responsibleGamblingService;
    }

    public PlinkoBoardDto GetBoard(PlinkoRiskLevel riskLevel, int rows)
    {
        ValidateRows(rows);
        var multipliers = GetMultipliers(riskLevel, rows);
        return new PlinkoBoardDto(rows, riskLevel, multipliers);
    }

    public async Task<PlinkoRoundDto> PlayAsync(Guid userId, PlinkoPlayRequest request, string? bearerToken)
    {
        if (request.Stake <= 0) throw new InvalidOperationException("Stake must be greater than zero.");
        ValidateRows(request.Rows);

        if (request.BoardFee < 0) throw new InvalidOperationException("Board fee cannot be negative.");

        var totalCost = request.Stake + request.BoardFee;
        if (_responsibleGamblingService != null)
        {
            var validation = await _responsibleGamblingService.ValidateStakeAsync(userId, totalCost);
            if (!validation.IsAllowed)
            {
                throw new InvalidOperationException(validation.Message ?? "Stake is blocked by responsible gambling limits.");
            }
        }

        var stakeAccepted = await _walletGateway.ProcessStakeAsync(userId, totalCost, bearerToken);
        if (!stakeAccepted) throw new InvalidOperationException("Insufficient wallet balance.");

        if (_responsibleGamblingService != null)
        {
            await _responsibleGamblingService.RecordActivityAsync(
                userId,
                new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Stake, totalCost));
        }

        var pathMoves = GeneratePath(request.Rows);
        var landingSlot = pathMoves.Count(move => move == 'R');
        var multiplier = GetMultipliers(request.RiskLevel, request.Rows)[landingSlot];
        var round = new PlinkoRound(userId, request.Stake, request.RiskLevel, request.Rows, landingSlot, multiplier, new string(pathMoves));

        await _roundRepository.AddAsync(round);
        return ToDto(round);
    }

    public async Task<PlinkoRoundDto> SettleRoundAsync(Guid userId, Guid roundId, string? bearerToken)
    {
        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null || round.UserId != userId)
        {
            throw new InvalidOperationException("Plinko round was not found.");
        }

        if (!round.IsPayoutSettled)
        {
            if (round.Payout > 0)
            {
                await _walletGateway.ProcessPayoutAsync(userId, round.Payout, bearerToken);
                if (_responsibleGamblingService != null)
                {
                    await _responsibleGamblingService.RecordActivityAsync(
                        userId,
                        new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Payout, round.Payout));
                }
            }

            round.MarkPayoutSettled();
            await _roundRepository.UpdateAsync(round);
        }

        return ToDto(round);
    }

    public async Task<IReadOnlyCollection<PlinkoRoundDto>> GetHistoryAsync(Guid userId, int limit)
    {
        var rounds = await _roundRepository.GetRecentForUserAsync(userId, limit);
        return rounds.Select(ToDto).ToArray();
    }

    private static char[] GeneratePath(int rows)
    {
        var path = new char[rows];
        for (var i = 0; i < rows; i++)
        {
            path[i] = RandomNumberGenerator.GetInt32(0, 2) == 0 ? 'L' : 'R';
        }

        return path;
    }

    private static decimal[] GetMultipliers(PlinkoRiskLevel riskLevel, int rows)
    {
        return Boards.GetOrAdd((riskLevel, rows), key => CreateMultipliers(key.Risk, key.Rows));
    }

    private static decimal[] CreateMultipliers(PlinkoRiskLevel riskLevel, int rows)
    {
        var edge = riskLevel switch
        {
            PlinkoRiskLevel.Low => 2.2m,
            PlinkoRiskLevel.Medium => 6.0m,
            PlinkoRiskLevel.High => 28.0m,
            _ => 6.0m
        };

        var center = riskLevel switch
        {
            PlinkoRiskLevel.Low => 0.55m,
            PlinkoRiskLevel.Medium => 0.18m,
            PlinkoRiskLevel.High => 0.0m,
            _ => 0.18m
        };

        var houseEdge = riskLevel switch
        {
            PlinkoRiskLevel.Low => 0.78m,
            PlinkoRiskLevel.Medium => 0.62m,
            PlinkoRiskLevel.High => 0.50m,
            _ => 0.62m
        };

        var curvePower = riskLevel switch
        {
            PlinkoRiskLevel.Low => 2.2,
            PlinkoRiskLevel.Medium => 2.8,
            PlinkoRiskLevel.High => 3.6,
            _ => 2.8
        };

        var values = Enumerable.Range(0, rows + 1)
            .Select(slot =>
            {
                var distanceFromCenter = Math.Abs(slot - rows / 2.0) / (rows / 2.0);
                var rawValue = center + (edge - center) * (decimal)Math.Pow(distanceFromCenter, curvePower);
                var value = rawValue * houseEdge;
                return Math.Round(value, 2);
            })
            .ToArray();

        return values;
    }

    private static void ValidateRows(int rows)
    {
        if (rows < 8 || rows > 16)
        {
            throw new InvalidOperationException("Rows must be between 8 and 16.");
        }
    }

    private static PlinkoRoundDto ToDto(PlinkoRound round)
    {
        return new PlinkoRoundDto(
            round.Id,
            round.Stake,
            round.RiskLevel,
            round.Rows,
            round.LandingSlot,
            round.Multiplier,
            round.Payout,
            round.IsPayoutSettled,
            round.Path,
            round.CreatedAtUtc);
    }
}
