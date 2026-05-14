using System;
using System.Collections.Generic;

namespace _67Bet.Odds.Application.DTOs;

public class LiveMatchStateDto
{
    public string MatchId { get; set; } = string.Empty;
    public string SportKey { get; set; } = string.Empty;
    public string CurrentTime { get; set; } = string.Empty;
    public string CurrentAction { get; set; } = string.Empty;
    public string CurrentZone { get; set; } = "Midfield"; // np. HomeDef, Midfield, AwayBox
    public int Momentum { get; set; } = 50; // 0-100 (50 to balans)
    public Dictionary<string, string> Score { get; set; } = new();
    public Dictionary<string, int> Statistics { get; set; } = new();
    public List<TimelineEventDto> TimelineEvents { get; set; } = new();
    public string? StreamUrl { get; set; } // URL do transmisji (Twitch, YouTube, Iframe)
}

public class TimelineEventDto
{
    public string Type { get; set; } = string.Empty; // Goal, Card, Corner
    public string Minute { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Team { get; set; } = "Home"; // Home or Away
}
