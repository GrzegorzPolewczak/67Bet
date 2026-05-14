using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace _67Bet.Odds.Application.DTOs;

public class ExternalEventDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; } = string.Empty;

    [JsonPropertyName("sport_title")]
    public string SportTitle { get; set; } = string.Empty;

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = string.Empty;

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = string.Empty;

    public string? StreamUrl { get; set; }

    [JsonPropertyName("bookmakers")]
    public List<BookmakerDto> Bookmakers { get; set; } = new();
}

public class BookmakerDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("markets")]
    public List<ExternalMarketDto> Markets { get; set; } = new();
}

public class ExternalMarketDto
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("outcomes")]
    public List<ExternalOutcomeDto> Outcomes { get; set; } = new();
}

public class ExternalOutcomeDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }
}

public class SyncResult
{
    public int EventsProcessed { get; set; }
    public int NewEventsAdded { get; set; }
    public List<string> Errors { get; set; } = new();
}
