using System;
using System.Collections.Generic;
using _67Bet.Odds.Application.DTOs;

namespace _67Bet.Odds.Api.Services;

public class TennisSimulator : BaseMatchSimulator
{
    public override string SportKey => "tennis";

    protected override void Simulate(LiveMatchStateDto match)
    {
        int chance = Random.Next(100);
        bool isHomeServer = match.Momentum >= 50;

        // Inicjalizacja specyficznych statystyk jeśli ich nie ma
        if (!match.Statistics.ContainsKey("Aces")) match.Statistics["Aces"] = 0;
        if (!match.Statistics.ContainsKey("DoubleFaults")) match.Statistics["DoubleFaults"] = 0;
        if (!match.Statistics.ContainsKey("GamesHome")) match.Statistics["GamesHome"] = 0;
        if (!match.Statistics.ContainsKey("GamesAway")) match.Statistics["GamesAway"] = 0;
        if (!match.Statistics.ContainsKey("WinProbHome")) match.Statistics["WinProbHome"] = 50;

        // Inicjalizacja wyniku (Setów) jeśli nie ma
        if (!match.Score.ContainsKey("Home")) match.Score["Home"] = "0";
        if (!match.Score.ContainsKey("Away")) match.Score["Away"] = "0";

        if (chance < 20) // Częstsze Asy (20% szans na akcję typu As)
        {
            match.CurrentAction = "ACE!";
            match.Statistics["Aces"]++;
            AddTimelineEvent(match, "Ace", isHomeServer ? "Home" : "Away", "Brilliant Serve");
        }
        else if (chance < 35) // Podwójny błąd (15% szans)
        {
            match.CurrentAction = "Double Fault";
            match.Statistics["DoubleFaults"]++;
            match.Momentum = 100 - match.Momentum; // Zmiana przewagi
        }
        else if (chance < 70) // Rally
        {
            match.CurrentAction = "Intense Rally";
            match.CurrentZone = Random.Next(100) < 50 ? "HomeDef" : "AwayDef";
            
            // Losowa wygrana punktu po wymianie
            if (Random.Next(100) < 30)
            {
                string winningPointTeam = Random.Next(100) < match.Momentum ? "Home" : "Away";
                SimulatePointWin(match, winningPointTeam);
            }
        }
        else if (chance < 85) // Game Point
        {
            match.CurrentAction = "GAME POINT";
            if (Random.Next(100) < 50)
            {
                string winningGameTeam = isHomeServer ? "Home" : "Away";
                SimulateGameWin(match, winningGameTeam);
            }
        }
        else
        {
            match.CurrentAction = "Change of ends";
        }

        // Win Probability płynie za momentum
        match.Statistics["WinProbHome"] = Math.Clamp(match.Momentum + Random.Next(-5, 6), 5, 95);
    }

    private void SimulatePointWin(LiveMatchStateDto match, string team)
    {
        match.CurrentAction = $"Point for {team}";
        // W tenisie punkty to 15, 30, 40, ale my upraszczamy do statystyki wygranych piłek lub gemów
        if (Random.Next(100) < 20) SimulateGameWin(match, team);
    }

    private void SimulateGameWin(LiveMatchStateDto match, string team)
    {
        string key = team == "Home" ? "GamesHome" : "GamesAway";
        match.Statistics[key]++;
        match.CurrentAction = $"GAME {team}!";
        AddTimelineEvent(match, "Game", team, $"Game won by {team}");

        // Jeśli wygrał 6 gemów (uproszczone), wygrywa seta
        if (match.Statistics[key] >= 6)
        {
            int currentSets = int.Parse(match.Score[team]);
            match.Score[team] = (currentSets + 1).ToString();
            match.CurrentAction = $"SET {team}!!!";
            AddTimelineEvent(match, "Set", team, $"SET won by {team}");
            
            // Reset gemów po secie
            match.Statistics["GamesHome"] = 0;
            match.Statistics["GamesAway"] = 0;
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
        if (match.TimelineEvents.Count > 10) match.TimelineEvents.RemoveAt(10);
    }
}
