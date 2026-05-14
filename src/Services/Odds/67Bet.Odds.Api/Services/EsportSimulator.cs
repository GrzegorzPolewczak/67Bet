using System;
using System.Collections.Generic;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public class EsportSimulator : BaseMatchSimulator
{
    public override string SportKey => "esport";

    protected override void Simulate(LiveMatchStateDto match)
    {
        bool isCsgo = match.SportKey.Contains("csgo", StringComparison.OrdinalIgnoreCase);
        int chance = Random.Next(100);

        if (isCsgo)
        {
            SimulateCsgo(match, chance);
        }
        else
        {
            SimulateMoba(match, chance);
        }

        if (string.IsNullOrEmpty(match.StreamUrl))
        {
            match.StreamUrl = "https://player.twitch.tv/?channel=gaules&parent=localhost"; 
        }
    }

    private void SimulateCsgo(LiveMatchStateDto match, int chance)
    {
        // Inicjalizacja statystyk CS:GO
        if (!match.Statistics.ContainsKey("RoundsHome")) match.Statistics["RoundsHome"] = 0;
        if (!match.Statistics.ContainsKey("RoundsAway")) match.Statistics["RoundsAway"] = 0;
        if (!match.Statistics.ContainsKey("EconomyHome")) match.Statistics["EconomyHome"] = 20;
        if (!match.Statistics.ContainsKey("EconomyAway")) match.Statistics["EconomyAway"] = 20;
        if (!match.Statistics.ContainsKey("BombPlanted")) match.Statistics["BombPlanted"] = 0;

        if (match.Statistics["BombPlanted"] == 1)
        {
            match.CurrentAction = "BOMB TICKING...";
            if (chance < 20) 
            {
                match.Statistics["BombPlanted"] = 0;
                match.CurrentAction = "BOMB DEFUSED";
                AddTimelineEvent(match, "Defuse", "Away", "Counter-Terrorists defused the bomb");
                match.Statistics["RoundsAway"]++;
            }
            else if (chance < 40)
            {
                match.Statistics["BombPlanted"] = 0;
                match.CurrentAction = "BOMB EXPLODED";
                AddTimelineEvent(match, "Explosion", "Home", "Terrorists won by explosion");
                match.Statistics["RoundsHome"]++;
            }
        }
        else if (chance < 15) // Kill
        {
            string team = Random.Next(100) < match.Momentum ? "Home" : "Away";
            match.CurrentAction = $"KILL SECURED BY {team.ToUpper()}";
            AddTimelineEvent(match, "Kill", team, $"{team} eliminated an opponent");
            match.Momentum = team == "Home" ? Math.Min(90, match.Momentum + 5) : Math.Max(10, match.Momentum - 5);
        }
        else if (chance < 25) // Bomb Plant
        {
            match.CurrentAction = "BOMB PLANTED";
            match.Statistics["BombPlanted"] = 1;
            match.CurrentZone = "AwayBox"; // Cele na mapie
            AddTimelineEvent(match, "Plant", "Home", "The bomb has been planted");
        }
        else
        {
            match.CurrentAction = "Tactical Maneuvers";
            match.Momentum = Math.Clamp(match.Momentum + Random.Next(-3, 4), 30, 70);
        }

        // Symulacja ekonomii ($k)
        match.Statistics["EconomyHome"] = Math.Clamp(match.Statistics["EconomyHome"] + Random.Next(-2, 5), 0, 80);
        match.Statistics["EconomyAway"] = Math.Clamp(match.Statistics["EconomyAway"] + Random.Next(-2, 5), 0, 80);
        
        // Aktualizacja wyniku w Score (Rundy)
        match.Score["Home"] = match.Statistics["RoundsHome"].ToString();
        match.Score["Away"] = match.Statistics["RoundsAway"].ToString();
    }

    private void SimulateMoba(LiveMatchStateDto match, int chance)
    {
        if (!match.Statistics.ContainsKey("Objectives")) match.Statistics["Objectives"] = 0;
        if (!match.Statistics.ContainsKey("GoldLead")) match.Statistics["GoldLead"] = 0;
        if (!match.Statistics.ContainsKey("MapControl")) match.Statistics["MapControl"] = 50;

        if (chance < 20) // Kill
        {
            string team = Random.Next(100) < match.Momentum ? "Home" : "Away";
            int currentKills = int.Parse(match.Score[team]);
            match.Score[team] = (currentKills + 1).ToString();
            match.CurrentAction = "PLAYER ELIMINATED";
            AddTimelineEvent(match, "Kill", team, $"{team} secured a kill");
            match.Momentum = team == "Home" ? Math.Min(90, match.Momentum + 10) : Math.Max(10, match.Momentum - 10);
        }
        else if (chance < 40) // Objective
        {
            match.CurrentAction = "Objective Contest";
            if (Random.Next(100) < 30)
            {
                string team = match.Momentum >= 50 ? "Home" : "Away";
                match.CurrentAction = "OBJECTIVE TAKEN";
                match.Statistics["Objectives"]++;
                AddTimelineEvent(match, "Objective", team, $"{team} took a major objective");
            }
        }
        else
        {
            match.CurrentAction = "Farming / Positioning";
            match.Momentum = Math.Clamp(match.Momentum + Random.Next(-5, 6), 20, 80);
        }

        match.Statistics["GoldLead"] = (match.Momentum - 50) / 2;
        match.Statistics["MapControl"] = match.Momentum;
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
