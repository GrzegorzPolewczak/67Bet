using System.Collections.Generic;

namespace _67Bet.Odds.Application.DTOs;

public class LiveMatchStateDto
{
    public string MatchId { get; set; } = string.Empty;
    public string SportKey { get; set; } = string.Empty;
    public string CurrentTime { get; set; } = string.Empty;
    public string CurrentAction { get; set; } = string.Empty;
    public Dictionary<string, string> Score { get; set; } = new();
    public Dictionary<string, int> Statistics { get; set; } = new();
}
