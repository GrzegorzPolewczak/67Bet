using System;
using System.Collections.Generic;
using System.Linq;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public class SoccerSimulator : BaseMatchSimulator
{
    public override string SportKey => "soccer";

    private enum MatchPhase
    {
        BuildUp,
        Attack,
        DangerousAttack,
        SetPiece
    }

    protected override void Simulate(LiveMatchStateDto match)
    {
        // 1. Określenie, kto przy piłce (Momentum)
        // Momentum > 50 -> Home, < 50 -> Away
        bool isHomePossession = match.Momentum >= 50;

        // 2. Losowa zmiana fazy i strefy
        int chance = Random.Next(100);

        if (chance < 15) // Zmiana posiadania
        {
            match.Momentum = 100 - match.Momentum;
            match.CurrentAction = "Possession Change";
            match.CurrentZone = "Midfield";
        }
        else if (chance < 40) // Przejście do ataku
        {
            match.CurrentAction = isHomePossession ? "Home Attack" : "Away Attack";
            match.CurrentZone = isHomePossession ? "AwayDef" : "HomeDef";
            match.Momentum = isHomePossession ? Math.Min(90, match.Momentum + 5) : Math.Max(10, match.Momentum - 5);
        }
        else if (chance < 60) // Groźny atak
        {
            match.CurrentAction = isHomePossession ? "Dangerous Attack!" : "Dangerous Attack!";
            match.CurrentZone = isHomePossession ? "AwayBox" : "HomeBox";
            match.Momentum = isHomePossession ? 85 : 15;
        }
        else if (chance < 70) // Rzut rożny / wolny
        {
            match.CurrentAction = "Set Piece";
            AddTimelineEvent(match, "Corner", isHomePossession ? "Home" : "Away", "Corner Kick");
            match.Statistics["Corners"]++;
        }
        else if (chance < 75 && match.CurrentZone.EndsWith("Box")) // Szansa na GOLA (tylko z pola karnego)
        {
            string scoringTeam = isHomePossession ? "Home" : "Away";
            int currentScore = int.Parse(match.Score[scoringTeam]);
            match.Score[scoringTeam] = (currentScore + 1).ToString();
            match.CurrentAction = "GOAL!!!";

            AddTimelineEvent(match, "Goal", scoringTeam, $"Goal scored by {scoringTeam}!");

            // Reset po golu
            match.CurrentZone = "Midfield";
            match.Momentum = 50;
        }
        else
        {
            match.CurrentAction = "Safe Possession";
            match.CurrentZone = "Midfield";
        }

        // Statystyki
        match.Statistics["PossessionHome"] = match.Momentum;

        // Przykładowy stream (np. kanał informacyjny sportowy)
        if (string.IsNullOrEmpty(match.StreamUrl))
        {
            match.StreamUrl = "https://www.youtube.com/embed/jfKfPfyJRdk?autoplay=1&mute=1";
        }
    }

    private void AddTimelineEvent(LiveMatchStateDto match, string type, string team, string desc)
    {
        match.TimelineEvents.Insert(0, new TimelineEventDto
        {
            Type = type,
            Minute = match.CurrentTime.Split(':')[0] + "'",
            Team = team,
            Description = desc
        });

        // Trzymamy max 10 zdarzeń
        if (match.TimelineEvents.Count > 10) match.TimelineEvents.RemoveAt(10);
    }
}
