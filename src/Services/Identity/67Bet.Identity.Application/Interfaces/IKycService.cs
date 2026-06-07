using System;
using System.Threading.Tasks;

namespace _67Bet.Identity.Application.Interfaces
{
    public interface IKycService
    {
        Task<Guid> GenerateSessionAsync(Guid userId);
        Task CompleteSessionAsync(Guid sessionId);
    }
}
