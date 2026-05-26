using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Identity.Domain.Enums
{
    public enum KycSessionStatus
    {
        Pending,
        Completed,
        Failed
    }
}

namespace _67Bet.Identity.Domain.Entities
{
    using _67Bet.Identity.Domain.Enums;

    public class KycSession : BaseEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public KycSessionStatus Status { get; private set; }

        public KycSession(Guid userId)
        {
            UserId = userId;
            Status = KycSessionStatus.Pending;
        }

        public void Complete()
        {
            Status = KycSessionStatus.Completed;
        }

        public void Fail()
        {
            Status = KycSessionStatus.Failed;
        }
    }
}
