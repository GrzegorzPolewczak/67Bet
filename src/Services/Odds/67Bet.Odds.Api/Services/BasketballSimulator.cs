using System;
using System.Collections.Generic;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public class BasketballSimulator : BaseMatchSimulator
{
    public override string SportKey => "basketball";

    protected override void Simulate(LiveMatchStateDto match)
    {
        bool isHomePossession = match.Momentum >= 50;
        int chance = Random.Next(100);

        // Inicjalizacja specyficznych statystyk
        if (!match.Statistics.ContainsKey("ThreePointers")) match.Statistics["ThreePointers"] = 0;
        if (!match.Statistics.ContainsKey("Rebounds")) match.Statistics["Rebounds"] = 0;
        if (!match.Statistics.ContainsKey("FGPercentHome")) match.Statistics["FGPercentHome"] = 45;
        if (!match.Statistics.ContainsKey("FGPercentAway")) match.Statistics["FGPercentAway"] = 42;

        if (chance < 20) // Turnover / Steal
        {
            match.Momentum = 100 - match.Momentum;
            match.CurrentAction = "Turnover";
            match.CurrentZone = "Midfield";
        }
        else if (chance < 70) // Scoring attempt
        {
            match.CurrentAction = isHomePossession ? "Home Attack" : "Away Attack";
            match.CurrentZone = isHomePossession ? "AwayBox" : "HomeBox";

            if (Random.Next(100) < 40) // SCORE!
            {
                string team = isHomePossession ? "Home" : "Away";
                bool isThree = Random.Next(100) < 30;
                int points = isThree ? 3 : 2;
                int currentScore = int.Parse(match.Score[team]);
                match.Score[team] = (currentScore + points).ToString();
                match.CurrentAction = isThree ? "3-POINTER!" : "Layup Score";

                if (isThree) match.Statistics["ThreePointers"]++;
                AddTimelineEvent(match, "Score", team, $"{points} pts by {team}");
            }
        }
        else if (chance < 85)
        {
            match.Statistics["Rebounds"]++;
            match.CurrentAction = "Rebound secured";
        }
        else
        {
            match.CurrentAction = "Defense building";
            match.CurrentZone = "Midfield";
        }

        // Płynna zmiana skuteczności rzutów
        match.Statistics["FGPercentHome"] = Math.Clamp(match.Statistics["FGPercentHome"] + Random.Next(-1, 2), 30, 60);
        match.Statistics["FGPercentAway"] = Math.Clamp(match.Statistics["FGPercentAway"] + Random.Next(-1, 2), 30, 60);
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
        if (match.TimelineEvents.Count > 10) match.TimelineEvents.RemoveAt(10);
    }
}
