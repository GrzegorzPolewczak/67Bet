using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Domain.Entities
{
    public class ReferralCode : BaseEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public string Code { get; private set; }
        public int UsageCount { get; private set; }

        public ReferralCode(Guid userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.");
            if (code.Length > 10) throw new ArgumentException("Code cannot exceed 10 characters.");

            UserId = userId;
            Code = code.ToUpper();
            UsageCount = 0;
        }

        public void IncrementUsage()
        {
            UsageCount++;
        }

        // EF Core
        private ReferralCode() { }
    }
}
