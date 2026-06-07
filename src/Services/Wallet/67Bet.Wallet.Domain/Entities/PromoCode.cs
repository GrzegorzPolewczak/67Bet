using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Domain.Entities
{
    public class PromoCode : BaseEntity, IAggregateRoot
    {
        public string Code { get; private set; } = null!;
        public decimal RewardAmount { get; private set; }
        public bool IsActive { get; private set; }

        public PromoCode(string code, decimal rewardAmount)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.");
            Code = code.ToUpper();
            RewardAmount = rewardAmount;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        // EF Core
        private PromoCode() { }
    }
}
