using System;
using System.Collections.Generic;

namespace _67Bet.Wallet.Application.DTOs
{
    public class ReferralStatusDto
    {
        public string? MyCode { get; set; }
        public int ReferralCount { get; set; }
        public int NextMilestone { get; set; }
        public bool HasUsedReferral { get; set; }
        public List<int> Milestones { get; set; } = new() { 5, 15, 25, 50, 100, 250 };
    }
}
