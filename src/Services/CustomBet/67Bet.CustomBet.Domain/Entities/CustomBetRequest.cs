/*
 * Plik zawiera encję CustomBetRequest oraz interfejs ICustomBetRepository.
 * Odpowiada za definicję wniosków o niestandardowe zakłady oraz kontrakt ich zapisu.
 */
using System;
using _67Bet.Shared.Kernel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _67Bet.CustomBet.Domain.Enums
{
    public enum RequestStatus
    {
        Pending,
        Reviewing,
        Accepted,
        Rejected
    }
}

namespace _67Bet.CustomBet.Domain.Entities
{
    using _67Bet.CustomBet.Domain.Enums;

    public class CustomBetRequest : BaseEntity, IAggregateRoot
    {
        public Guid UserId { get; private set; }
        public string Description { get; private set; } = null!;
        public decimal AiSuggestedOdds { get; private set; }
        public string? AiAnalysisNote { get; private set; }
        public string? AiRiskLevel { get; private set; }
        public string? AiCategory { get; private set; }
        public decimal? AdminFinalOdds { get; private set; }
        public RequestStatus Status { get; private set; }
        public string? AdminNote { get; private set; }

        public CustomBetRequest(Guid userId, string description)
        {
            UserId = userId;
            Description = description;
            Status = RequestStatus.Pending;
        }

        public void SetAiRecommendation(decimal odds, string note, string risk, string category)
        {
            AiSuggestedOdds = odds;
            AiAnalysisNote = note;
            AiRiskLevel = risk;
            AiCategory = category;
            Status = RequestStatus.Reviewing;
        }

        public void Accept(decimal finalOdds, string? note = null)
        {
            AdminFinalOdds = finalOdds;
            AdminNote = note;
            Status = RequestStatus.Accepted;
        }

        public void Reject(string reason)
        {
            AdminNote = reason;
            Status = RequestStatus.Rejected;
        }

        // EF Core
        private CustomBetRequest() { }
    }
}

namespace _67Bet.CustomBet.Domain.Repositories
{
    using _67Bet.CustomBet.Domain.Entities;

    public interface ICustomBetRepository : IRepository<CustomBetRequest>
    {
        Task<IEnumerable<CustomBetRequest>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<CustomBetRequest>> GetPendingRequestsAsync();
    }
}

