using System.Security.Cryptography;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.Roulette;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Application.Services;

public sealed class RouletteService : IRouletteService
{
    private readonly IRouletteRoundRepository _roundRepository;
    private readonly IRouletteWalletGateway _walletGateway;
    private readonly IResponsibleGamblingService? _responsibleGamblingService;

    public RouletteService(
        IRouletteRoundRepository roundRepository,
        IRouletteWalletGateway walletGateway,
        IResponsibleGamblingService? responsibleGamblingService = null)
    {
        _roundRepository = roundRepository;
        _walletGateway = walletGateway;
        _responsibleGamblingService = responsibleGamblingService;
    }

    public async Task<RouletteRoundDto> PlayAsync(Guid userId, RoulettePlayRequest request, string? bearerToken)
    {
        if (request.Bets == null || request.Bets.Count == 0)
            throw new InvalidOperationException("At least one bet is required.");
        if (request.Bets.Count > 10)
            throw new InvalidOperationException("Maximum 10 bets per round.");

        var totalStake = request.Bets.Sum(b => b.Stake);
        if (totalStake <= 0)
            throw new InvalidOperationException("Total stake must be greater than zero.");

        if (_responsibleGamblingService != null)
        {
            var validation = await _responsibleGamblingService.ValidateStakeAsync(userId, totalStake);
            if (!validation.IsAllowed)
                throw new InvalidOperationException(validation.Message ?? "Stake is blocked by responsible gambling limits.");
        }

        var stakeAccepted = await _walletGateway.ProcessStakeAsync(userId, totalStake, bearerToken);
        if (!stakeAccepted) throw new InvalidOperationException("Insufficient wallet balance.");

        if (_responsibleGamblingService != null)
        {
            await _responsibleGamblingService.RecordActivityAsync(
                userId,
                new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Stake, totalStake));
        }

        var spinResult = RandomNumberGenerator.GetInt32(0, 37);
        var bets = request.Bets
            .Select(b => new RouletteBet(b.BetType, b.ChosenNumber, b.Stake))
            .ToList();
        var round = new RouletteRound(userId, bets, spinResult);

        await _roundRepository.AddAsync(round);
        return ToDto(round);
    }

    public async Task<RouletteRoundDto> SettleRoundAsync(Guid userId, Guid roundId, string? bearerToken)
    {
        var round = await _roundRepository.GetByIdAsync(roundId);
        if (round == null || round.UserId != userId)
            throw new InvalidOperationException("Roulette round was not found.");

        if (!round.IsPayoutSettled)
        {
            if (round.TotalPayout > 0)
            {
                await _walletGateway.ProcessPayoutAsync(userId, round.TotalPayout, bearerToken);
                if (_responsibleGamblingService != null)
                {
                    await _responsibleGamblingService.RecordActivityAsync(
                        userId,
                        new RecordResponsibleGamblingActivityRequest(ResponsibleGamblingActivityType.Payout, round.TotalPayout));
                }
            }

            round.MarkPayoutSettled();
            await _roundRepository.UpdateAsync(round);
        }

        return ToDto(round);
    }

    public async Task<IReadOnlyCollection<RouletteRoundDto>> GetHistoryAsync(Guid userId, int limit)
    {
        var rounds = await _roundRepository.GetRecentForUserAsync(userId, limit);
        return rounds.Select(ToDto).ToArray();
    }

    private static RouletteRoundDto ToDto(RouletteRound round) =>
        new(
            round.Id,
            round.SpinResult,
            round.TotalStake,
            round.TotalPayout,
            round.IsPayoutSettled,
            round.CreatedAtUtc,
            round.Bets.Select(b => new RouletteBetDto(
                b.Id,
                b.BetType,
                b.ChosenNumber,
                b.Stake,
                b.IsWon,
                b.Payout)).ToArray());
}
