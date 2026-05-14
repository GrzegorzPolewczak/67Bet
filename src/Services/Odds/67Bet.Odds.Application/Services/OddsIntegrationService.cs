using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Odds.Application.DTOs;
using _67Bet.Odds.Application.Interfaces;
using _67Bet.Odds.Domain.Entities;
using _67Bet.Odds.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace _67Bet.Odds.Application.Services;

public class OddsIntegrationService : IOddsIntegrationService
{
    private readonly ITheOddsApiClient _apiClient;
    private readonly IPandaScoreApiClient _pandaScoreApiClient;
    private readonly IExternalEventRepository _eventRepository;
    private readonly ILogger<OddsIntegrationService> _logger;

    public OddsIntegrationService(
        ITheOddsApiClient apiClient,
        IPandaScoreApiClient pandaScoreApiClient,
        IExternalEventRepository eventRepository,
        ILogger<OddsIntegrationService> logger)
    {
        _apiClient = apiClient;
        _pandaScoreApiClient = pandaScoreApiClient;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public async Task<SyncResult> SyncExternalOddsAsync()
    {
        var result = new SyncResult();
        var sportsToSync = new[] { "upcoming", "basketball_nba", "mma_mixed_martial_arts", "soccer_epl", "soccer_spain_la_liga" };

        var allExternalEvents = new List<ExternalEventDto>();

        // 1. Pobieranie z The Odds API
        foreach (var sport in sportsToSync)
        {
            try
            {
                var events = await _apiClient.GetUpcomingEventsAsync(sport);
                allExternalEvents.AddRange(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing sport {Sport} from The Odds API", sport);
                result.Errors.Add($"Sport {sport}: {ex.Message}");
            }
        }

        // 2. Pobieranie z PandaScore API (Esport)
        try
        {
            var esportsEvents = await _pandaScoreApiClient.GetUpcomingEsportsMatchesAsync();
            allExternalEvents.AddRange(esportsEvents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing esports from PandaScore");
            result.Errors.Add($"PandaScore: {ex.Message}");
        }

        // 3. Przetwarzanie połączonej listy
        foreach (var extEvent in allExternalEvents)
        {
            result.EventsProcessed++;
            try
            {
                var existingEvent = await _eventRepository.GetByExternalIdAsync(extEvent.Id);
                if (existingEvent == null)
                {
                    var newEvent = new ExternalEvent(
                        extEvent.Id,
                        extEvent.SportKey,
                        $"{extEvent.HomeTeam} vs {extEvent.AwayTeam}",
                        extEvent.CommenceTime);

                    var primaryBookmaker = extEvent.Bookmakers.FirstOrDefault();
                    if (primaryBookmaker != null)
                    {
                        foreach (var mktDto in primaryBookmaker.Markets)
                        {
                            var market = new ExternalMarket(newEvent.Id, mktDto.Key);
                            foreach (var outDto in mktDto.Outcomes)
                            {
                                market.AddOutcome(new ExternalOutcome(market.Id, outDto.Name, outDto.Price));
                            }
                            newEvent.AddMarket(market);
                        }
                    }

                    await _eventRepository.AddAsync(newEvent);
                    result.NewEventsAdded++;
                }
                else
                {
                    existingEvent.UpdateInfo($"{extEvent.HomeTeam} vs {extEvent.AwayTeam}", extEvent.CommenceTime);
                    
                    var primaryBookmaker = extEvent.Bookmakers.FirstOrDefault();
                    if (primaryBookmaker != null)
                    {
                        foreach (var mktDto in primaryBookmaker.Markets)
                        {
                            var existingMarket = existingEvent.Markets.FirstOrDefault(m => m.Name == mktDto.Key);
                            if (existingMarket == null)
                            {
                                var market = new ExternalMarket(existingEvent.Id, mktDto.Key);
                                foreach (var outDto in mktDto.Outcomes)
                                {
                                    market.AddOutcome(new ExternalOutcome(market.Id, outDto.Name, outDto.Price));
                                }
                                existingEvent.AddMarket(market);
                            }
                            else
                            {
                                foreach (var outDto in mktDto.Outcomes)
                                {
                                    existingMarket.AddOutcome(new ExternalOutcome(existingMarket.Id, outDto.Name, outDto.Price));
                                }
                            }
                        }
                    }
                    await _eventRepository.UpdateAsync(existingEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {ExternalId}", extEvent.Id);
                result.Errors.Add($"Event {extEvent.Id}: {ex.Message}");
            }
        }

        return result;
    }

    private string FormatSportTitle(string sportKey)
    {
        if (string.IsNullOrEmpty(sportKey)) return "Unknown";
        var parts = sportKey.Split('_');
        if (parts.Length == 1) return char.ToUpper(sportKey[0]) + sportKey.Substring(1).ToLower();
        
        var formattedParts = parts.Skip(1).Select(p => 
        {
            var lower = p.ToLower();
            if (lower == "csgo") return "CS:GO";
            if (lower == "lol" || lower == "league" || lower == "legends") return "LoL";
            if (lower == "nba") return "NBA";
            if (lower == "ufc") return "UFC";
            return char.ToUpper(lower[0]) + lower.Substring(1);
        });
        
        return string.Join(" ", formattedParts);
    }

    public async Task<IEnumerable<ExternalEventDto>> GetEventsAsync()
    {
        var entities = await _eventRepository.GetAllActiveAsync();
        return entities.Select(e => new ExternalEventDto
        {
            Id = e.ExternalId,
            SportKey = e.SportKey,
            SportTitle = FormatSportTitle(e.SportKey),
            CommenceTime = e.StartTime,
            HomeTeam = e.Name.Split(" vs ").FirstOrDefault() ?? e.Name,
            AwayTeam = e.Name.Split(" vs ").LastOrDefault() ?? string.Empty,
            Bookmakers = new List<BookmakerDto>
            {
                new BookmakerDto
                {
                    Key = "internal",
                    Title = "67Bet Internal",
                    Markets = e.Markets.Select(m => new ExternalMarketDto
                    {
                        Key = m.Name,
                        Outcomes = m.Outcomes.Select(o => new ExternalOutcomeDto
                        {
                            Name = o.Name,
                            Price = o.Price
                        }).ToList()
                    }).ToList()
                }
            }
        });
    }
}
