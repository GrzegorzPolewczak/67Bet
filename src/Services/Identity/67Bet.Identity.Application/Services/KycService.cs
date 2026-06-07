using System;
using System.Threading.Tasks;
using _67Bet.Identity.Application.Interfaces;
using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;

namespace _67Bet.Identity.Application.Services
{
    public class KycService : IKycService
    {
        private readonly IKycSessionRepository _kycSessionRepository;
        private readonly IUserRepository _userRepository;

        public KycService(IKycSessionRepository kycSessionRepository, IUserRepository userRepository)
        {
            _kycSessionRepository = kycSessionRepository;
            _userRepository = userRepository;
        }

        public async Task<Guid> GenerateSessionAsync(Guid userId)
        {
            var session = new KycSession(userId);
            await _kycSessionRepository.AddAsync(session);
            return session.Id;
        }

        public async Task CompleteSessionAsync(Guid sessionId)
        {
            var session = await _kycSessionRepository.GetByIdAsync(sessionId);
            if (session != null)
            {
                session.Complete();
                await _kycSessionRepository.UpdateAsync(session);

                var user = await _userRepository.GetByIdAsync(session.UserId);
                if (user != null)
                {
                    user.VerifyKyc();
                    await _userRepository.UpdateAsync(user);
                }
            }
        }
    }
}
