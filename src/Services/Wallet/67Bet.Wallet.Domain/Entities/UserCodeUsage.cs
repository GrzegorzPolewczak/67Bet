using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Wallet.Domain.Entities
{
    public class UserCodeUsage : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid CodeId { get; private set; }
        public bool IsReferral { get; private set; } // true for ReferralCode, false for PromoCode

        public UserCodeUsage(Guid userId, Guid codeId, bool isReferral)
        {
            UserId = userId;
            CodeId = codeId;
            IsReferral = isReferral;
        }

        // EF Core
        private UserCodeUsage() { }
    }
}
