using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.Gamification;
using _67Bet.Betting.Domain.Enums;
using _67Bet.Betting.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace _67Bet.Betting.Application.Services;

public class GamificationService : IGamificationService
{
    private readonly IUserGamificationRepository _gamificationRepo;
    private readonly IAchievementRepository _achievementRepo;
    private readonly IUserAchievementRepository _userAchievementRepo;
    private readonly ILogger<GamificationService> _logger;

    public GamificationService(
        IUserGamificationRepository gamificationRepo,
        IAchievementRepository achievementRepo,
        IUserAchievementRepository userAchievementRepo,
        ILogger<GamificationService> logger)
    {
        _gamificationRepo = gamificationRepo;
        _achievementRepo = achievementRepo;
        _userAchievementRepo = userAchievementRepo;
        _logger = logger;
    }

    public async Task AwardXpForBetAsync(Guid userId, decimal stake)
    {
        var gamification = await GetOrCreateUserGamificationAsync(userId);
        long xpToAdd = (long)stake; // 1 XP per 1 unit of stake

        bool leveledUp = gamification.AddExperience(xpToAdd);
        await _gamificationRepo.UpdateAsync(gamification);

        await UpdateAchievementProgressAsync(userId, AchievementType.TotalBets, 1);

        _logger.LogInformation("Awarded {XP} XP to user {UserId} for placing a bet. Level Up: {LeveledUp}", xpToAdd, userId, leveledUp);

        // TODO: Trigger SignalR notification
    }

    public async Task AwardXpForWinAsync(Guid userId, decimal stake, decimal odds)
    {
        var gamification = await GetOrCreateUserGamificationAsync(userId);
        // Formula: XP = Stawka * (Kurs - 1) * 0.5
        long xpToAdd = (long)(stake * (odds - 1) * 0.5m);

        if (xpToAdd > 0)
        {
            bool leveledUp = gamification.AddExperience(xpToAdd);
            await _gamificationRepo.UpdateAsync(gamification);
            _logger.LogInformation("Awarded {XP} XP to user {UserId} for winning a bet.", xpToAdd, userId);
        }

        await UpdateAchievementProgressAsync(userId, AchievementType.TotalWinnings, stake * odds);
        await UpdateAchievementProgressAsync(userId, AchievementType.HighOdds, odds);
    }

    public async Task ProcessDailyLoginAsync(Guid userId)
    {
        var gamification = await GetOrCreateUserGamificationAsync(userId);
        if (gamification.ProcessLogin(DateTime.UtcNow))
        {
            gamification.AddExperience(20); // 20 XP for daily login
            await _gamificationRepo.UpdateAsync(gamification);
            await UpdateAchievementProgressAsync(userId, AchievementType.LoginStreak, 1);
            _logger.LogInformation("Awarded 20 XP to user {UserId} for daily login.", userId);
        }
    }

    public async Task AwardXpForPlinkoPlayAsync(Guid userId, decimal stake, decimal payout)
    {
        var gamification = await GetOrCreateUserGamificationAsync(userId);
        long xpToAdd = (long)stake; // 1 XP per 1 unit of stake
        if (payout > stake)
        {
            xpToAdd += (long)((payout - stake) * 0.5m);
        }

        if (xpToAdd > 0)
        {
            bool leveledUp = gamification.AddExperience(xpToAdd);
            await _gamificationRepo.UpdateAsync(gamification);
            _logger.LogInformation("Awarded {XP} XP to user {UserId} for Plinko play. Level Up: {LeveledUp}", xpToAdd, userId, leveledUp);
        }

        await UpdateAchievementProgressAsync(userId, AchievementType.PlinkoRounds, 1);
        if (payout > 0)
        {
            await UpdateAchievementProgressAsync(userId, AchievementType.TotalWinnings, payout);
        }
    }

    public async Task AwardXpForRoulettePlayAsync(Guid userId, decimal stake, decimal payout, int spinResult)
    {
        var gamification = await GetOrCreateUserGamificationAsync(userId);
        long xpToAdd = (long)stake; // 1 XP per 1 unit of stake
        if (payout > stake)
        {
            xpToAdd += (long)((payout - stake) * 0.5m);
        }

        if (xpToAdd > 0)
        {
            bool leveledUp = gamification.AddExperience(xpToAdd);
            await _gamificationRepo.UpdateAsync(gamification);
            _logger.LogInformation("Awarded {XP} XP to user {UserId} for Roulette play. Level Up: {LeveledUp}", xpToAdd, userId, leveledUp);
        }

        await UpdateAchievementProgressAsync(userId, AchievementType.RouletteSpins, 1);
        if (payout > 0)
        {
            await UpdateAchievementProgressAsync(userId, AchievementType.TotalWinnings, payout);
        }

        if (spinResult == 0)
        {
            await UpdateAchievementProgressAsync(userId, AchievementType.GreenRoulette, 1);
        }
    }

    public async Task AwardXpForKycVerificationAsync(Guid userId)
    {
        var achievements = (await _achievementRepo.GetAllAsync()).Where(a => a.Type == AchievementType.KycVerification);
        foreach (var a in achievements)
        {
            var ua = await _userAchievementRepo.GetSpecificAsync(userId, a.Id);
            if (ua == null || !ua.IsUnlocked)
            {
                if (ua == null)
                {
                    ua = new UserAchievement(userId, a.Id);
                    await _userAchievementRepo.AddAsync(ua);
                }

                bool unlocked = ua.UpdateProgress(a.Threshold, a.Threshold);
                if (unlocked)
                {
                    await _userAchievementRepo.UpdateAsync(ua);
                    _logger.LogInformation("User {UserId} unlocked KycVerification achievement.", userId);

                    var gamification = await GetOrCreateUserGamificationAsync(userId);
                    bool leveledUp = gamification.AddExperience(250);
                    await _gamificationRepo.UpdateAsync(gamification);
                    _logger.LogInformation("Awarded 250 XP to user {UserId} for KYC verification. Level Up: {LeveledUp}", userId, leveledUp);
                }
            }
        }
    }

    public async Task<UserGamificationDto> GetUserProgressAsync(Guid userId)
    {
        var g = await GetOrCreateUserGamificationAsync(userId);
        long currentLevelXp = g.CalculateXpForLevel(g.CurrentLevel);
        long nextLevelXp = g.CalculateXpForLevel(g.CurrentLevel + 1);

        double progress = 0;
        if (nextLevelXp > currentLevelXp)
        {
            progress = (double)(g.ExperiencePoints - currentLevelXp) / (nextLevelXp - currentLevelXp) * 100;
        }

        return new UserGamificationDto(
            g.UserId,
            g.ExperiencePoints,
            g.CurrentLevel,
            nextLevelXp,
            Math.Round(progress, 2)
        );
    }

    public async Task<IEnumerable<UserAchievementDto>> GetUserAchievementsAsync(Guid userId)
    {
        var allAchievements = await _achievementRepo.GetAllAsync();
        var userAchievements = await _userAchievementRepo.GetByUserIdAsync(userId);

        var result = new List<UserAchievementDto>();
        foreach (var a in allAchievements)
        {
            var ua = userAchievements.FirstOrDefault(x => x.AchievementId == a.Id);
            result.Add(new UserAchievementDto(
                a.Id,
                a.Name,
                a.Description,
                ua?.CurrentProgress ?? 0,
                a.Threshold,
                ua?.IsUnlocked ?? false,
                ua?.UnlockedAt,
                a.IconUrl,
                a.Type.ToString()
            ));
        }

        return result;
    }

    private async Task<UserGamification> GetOrCreateUserGamificationAsync(Guid userId)
    {
        var gamification = await _gamificationRepo.GetByUserIdAsync(userId);
        if (gamification == null)
        {
            gamification = new UserGamification(userId);
            await _gamificationRepo.AddAsync(gamification);
        }
        return gamification;
    }

    private async Task UpdateAchievementProgressAsync(Guid userId, AchievementType type, decimal value)
    {
        var achievements = (await _achievementRepo.GetAllAsync()).Where(a => a.Type == type);
        foreach (var a in achievements)
        {
            var ua = await _userAchievementRepo.GetSpecificAsync(userId, a.Id);
            if (ua == null)
            {
                ua = new UserAchievement(userId, a.Id);
                await _userAchievementRepo.AddAsync(ua);
            }

            bool unlocked = false;
            if (type == AchievementType.HighOdds)
            {
                // For high odds, we check if the new value is higher than current progress
                if (value > ua.CurrentProgress)
                {
                    unlocked = ua.UpdateProgress(value, a.Threshold);
                }
            }
            else
            {
                unlocked = ua.AddProgress(value, a.Threshold);
            }

            if (unlocked)
            {
                _logger.LogInformation("User {UserId} unlocked achievement: {AchievementName}", userId, a.Name);
                // TODO: SignalR notification
            }

            await _userAchievementRepo.UpdateAsync(ua);
        }
    }
}
