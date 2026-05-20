namespace _67Bet.Wallet.Application.DTOs
{
    public class PromoCodeDto
    {
        public string Code { get; set; } = null!;
        public decimal RewardAmount { get; set; }
        public bool IsActive { get; set; }
    }
}
