using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Application.DTOs;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Wallet.Domain.Repositories;

namespace _67Bet.Wallet.Application.Services
{
    public class ReferralService : IReferralService
    {
        private readonly IReferralCodeRepository _referralRepository;
        private readonly IPromoCodeRepository _promoRepository;
        private readonly IUserCodeUsageRepository _usageRepository;
        private readonly IWalletService _walletService;

        private readonly int[] _milestones = { 5, 15, 25, 50, 100, 250 };
        private readonly decimal _referralReward = 20.00m;

        public ReferralService(
            IReferralCodeRepository referralRepository,
            IPromoCodeRepository promoRepository,
            IUserCodeUsageRepository usageRepository,
            IWalletService walletService)
        {
            _referralRepository = referralRepository;
            _promoRepository = promoRepository;
            _usageRepository = usageRepository;
            _walletService = walletService;
        }

        public async Task CreateCreatorCodeAsync(Guid userId, string code)
        {
            var existing = await _referralRepository.GetByUserIdAsync(userId);
            if (existing != null) throw new InvalidOperationException("User already has a creator code.");

            var codeExists = await _referralRepository.GetByCodeAsync(code);
            if (codeExists != null) throw new InvalidOperationException("This code is already taken.");

            var referralCode = new ReferralCode(userId, code);
            await _referralRepository.AddAsync(referralCode);
        }

        public async Task ApplyCodeAsync(Guid userId, string code)
        {
            code = code.ToUpper();

            // 1. SprawdĹş czy to PromoCode
            var promo = await _promoRepository.GetByCodeAsync(code);
            if (promo != null)
            {
                if (!promo.IsActive) throw new InvalidOperationException("Promo code is inactive.");

                var alreadyUsed = await _usageRepository.HasUsedCodeAsync(userId, promo.Id);
                if (alreadyUsed) throw new InvalidOperationException("This code has already been used on this account.");

                await _walletService.DepositFreebetAsync(userId, promo.RewardAmount);
                await _usageRepository.AddAsync(new UserCodeUsage(userId, promo.Id, false));
                return;
            }

            // 2. SprawdĹş czy to CreatorCode
            var creatorCode = await _referralRepository.GetByCodeAsync(code);
            if (creatorCode != null)
            {
                if (creatorCode.UserId == userId) throw new InvalidOperationException("You cannot use your own referral code.");

                var hasUsedReferral = await _usageRepository.HasUsedAnyReferralAsync(userId);
                if (hasUsedReferral) throw new InvalidOperationException("You can only use a referral code once.");

                await _walletService.DepositFreebetAsync(userId, _referralReward);
                await _usageRepository.AddAsync(new UserCodeUsage(userId, creatorCode.Id, true));

                creatorCode.IncrementUsage();
                await _referralRepository.UpdateAsync(creatorCode);

                // SprawdĹş kamienie milowe dla twĂłrcy
                await CheckMilestonesAsync(creatorCode);
                return;
            }

            throw new InvalidOperationException("Invalid code.");
        }

        public async Task<ReferralStatusDto> GetReferralStatusAsync(Guid userId)
        {
            var code = await _referralRepository.GetByUserIdAsync(userId);
            var hasUsedReferral = await _usageRepository.HasUsedAnyReferralAsync(userId);

            string? usedReferralCode = null;
            if (hasUsedReferral)
            {
                var usage = await _usageRepository.GetUsedReferralAsync(userId);
                if (usage != null)
                {
                    var refCode = await _referralRepository.GetByIdAsync(usage.CodeId);
                    usedReferralCode = refCode?.Code;
                }
            }

            return new ReferralStatusDto
            {
                MyCode = code?.Code,
                ReferralCount = code?.UsageCount ?? 0,
                HasUsedReferral = hasUsedReferral,
                UsedReferralCode = usedReferralCode,
                NextMilestone = _milestones.FirstOrDefault(m => m > (code?.UsageCount ?? 0))
            };
        }

        public async Task CreatePromoCodeAsync(string code, decimal reward)
        {
            var existing = await _promoRepository.GetByCodeAsync(code);
            if (existing != null) throw new InvalidOperationException("Promo code already exists.");

            var promo = new PromoCode(code, reward);
            await _promoRepository.AddAsync(promo);
        }

        public async Task DeactivatePromoCodeAsync(string code)
        {
            var promo = await _promoRepository.GetByCodeAsync(code);
            if (promo == null) throw new InvalidOperationException("Promo code does not exist.");

            promo.Deactivate();
            await _promoRepository.UpdateAsync(promo);
        }

        public async Task ActivatePromoCodeAsync(string code)
        {
            var promo = await _promoRepository.GetByCodeAsync(code);
            if (promo == null) throw new InvalidOperationException("Promo code does not exist.");

            promo.Activate();
            await _promoRepository.UpdateAsync(promo);
        }

        public async Task<IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync()
        {
            var codes = await _promoRepository.GetAllAsync();
            return codes.Select(c => new PromoCodeDto
            {
                Code = c.Code,
                RewardAmount = c.RewardAmount,
                IsActive = c.IsActive
            });
        }

        private async Task CheckMilestonesAsync(ReferralCode code)
        {
            if (_milestones.Contains(code.UsageCount))
            {
                decimal reward = code.UsageCount switch
                {
                    5 => 50m,
                    15 => 100m,
                    25 => 150m,
                    50 => 300m,
                    100 => 600m,
                    250 => 1500m,
                    _ => 0m
                };

                if (reward > 0)
                {
                    await _walletService.DepositFreebetAsync(code.UserId, reward);
                }
            }
        }
    }
}
