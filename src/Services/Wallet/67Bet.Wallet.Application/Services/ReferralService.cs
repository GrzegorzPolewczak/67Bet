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
            if (existing != null) throw new InvalidOperationException("UĹĽytkownik posiada juĹĽ kod twĂłrcy.");

            var codeExists = await _referralRepository.GetByCodeAsync(code);
            if (codeExists != null) throw new InvalidOperationException("Ten kod jest juĹĽ zajÄ™ty.");

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
                if (!promo.IsActive) throw new InvalidOperationException("Kod promocyjny jest nieaktywny.");
                
                var alreadyUsed = await _usageRepository.HasUsedCodeAsync(userId, promo.Id);
                if (alreadyUsed) throw new InvalidOperationException("Kod zostaĹ‚ juĹĽ wykorzystany na tym koncie.");

                await _walletService.DepositFreebetAsync(userId, promo.RewardAmount);
                await _usageRepository.AddAsync(new UserCodeUsage(userId, promo.Id, false));
                return;
            }

            // 2. SprawdĹş czy to CreatorCode
            var creatorCode = await _referralRepository.GetByCodeAsync(code);
            if (creatorCode != null)
            {
                if (creatorCode.UserId == userId) throw new InvalidOperationException("Nie moĹĽesz uĹĽyÄ‡ wĹ‚asnego kodu.");

                var hasUsedReferral = await _usageRepository.HasUsedAnyReferralAsync(userId);
                if (hasUsedReferral) throw new InvalidOperationException("MoĹĽesz uĹĽyÄ‡ kodu polecenia tylko raz.");

                await _walletService.DepositFreebetAsync(userId, _referralReward);
                await _usageRepository.AddAsync(new UserCodeUsage(userId, creatorCode.Id, true));

                creatorCode.IncrementUsage();
                await _referralRepository.UpdateAsync(creatorCode);

                // SprawdĹş kamienie milowe dla twĂłrcy
                await CheckMilestonesAsync(creatorCode);
                return;
            }

            throw new InvalidOperationException("NieprawidĹ‚owy kod.");
        }

        public async Task<ReferralStatusDto> GetReferralStatusAsync(Guid userId)
        {
            var code = await _referralRepository.GetByUserIdAsync(userId);
            var hasUsedReferral = await _usageRepository.HasUsedAnyReferralAsync(userId);

            return new ReferralStatusDto
            {
                MyCode = code?.Code,
                ReferralCount = code?.UsageCount ?? 0,
                HasUsedReferral = hasUsedReferral,
                NextMilestone = _milestones.FirstOrDefault(m => m > (code?.UsageCount ?? 0))
            };
        }

        public async Task CreatePromoCodeAsync(string code, decimal reward)
        {
            var existing = await _promoRepository.GetByCodeAsync(code);
            if (existing != null) throw new InvalidOperationException("Kod promo juĹĽ istnieje.");

            var promo = new PromoCode(code, reward);
            await _promoRepository.AddAsync(promo);
        }

        public async Task DeactivatePromoCodeAsync(string code)
        {
            var promo = await _promoRepository.GetByCodeAsync(code);
            if (promo == null) throw new InvalidOperationException("Kod promo nie istnieje.");

            promo.Deactivate();
            await _promoRepository.UpdateAsync(promo);
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
