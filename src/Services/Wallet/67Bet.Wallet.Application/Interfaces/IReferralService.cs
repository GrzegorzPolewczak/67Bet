using System;
using System.Threading.Tasks;
using _67Bet.Wallet.Application.DTOs;

namespace _67Bet.Wallet.Application.Interfaces
{
    public interface IReferralService
    {
        Task CreateCreatorCodeAsync(Guid userId, string code);
        Task ApplyCodeAsync(Guid userId, string code);
        Task<ReferralStatusDto> GetReferralStatusAsync(Guid userId);
        Task DeactivatePromoCodeAsync(string code);
        Task ActivatePromoCodeAsync(string code);
        Task CreatePromoCodeAsync(string code, decimal reward);
        Task<System.Collections.Generic.IEnumerable<PromoCodeDto>> GetAllPromoCodesAsync();
    }
}
