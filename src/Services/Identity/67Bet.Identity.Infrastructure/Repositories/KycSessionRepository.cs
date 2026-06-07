using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;
using _67Bet.Identity.Infrastructure.Persistence;
using _67Bet.Shared.Kernel;

namespace _67Bet.Identity.Infrastructure.Repositories
{
    public class KycSessionRepository : EFRepository<KycSession, IdentityDbContext>, IKycSessionRepository
    {
        public KycSessionRepository(IdentityDbContext context) : base(context) { }
    }
}
