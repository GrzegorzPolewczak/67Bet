using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Betting.Application.DTOs;
using _67Bet.Betting.Application.Interfaces;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.Betting.Application.Services
{
    public class VirtualRacingService : IVirtualRacingService
    {
        private readonly IVirtualRaceRepository _raceRepository;
        private readonly IHorseRepository _horseRepository;

        public VirtualRacingService(IVirtualRaceRepository raceRepository, IHorseRepository horseRepository)
        {
            _raceRepository = raceRepository;
            _horseRepository = horseRepository;
        }

        public async Task<IEnumerable<VirtualRaceDto>> GetActiveRacesAsync()
        {
            var races = await _raceRepository.GetActiveRacesAsync();
            return races.Select(MapToDto);
        }

        public async Task<VirtualRaceDto> GenerateRaceAsync()
        {
            var allHorses = (await _horseRepository.GetAllAsync()).ToList();
            if (allHorses.Count < 2)
            {
                throw new InvalidOperationException("Not enough horses to generate a race.");
            }

            var random = new Random();
            var raceHorses = allHorses.OrderBy(h => random.Next()).Take(Math.Min(8, allHorses.Count)).ToList();

            var raceName = $"Virtual Derby {DateTime.UtcNow:HH:mm:ss}";
            var startTime = DateTime.UtcNow.AddMinutes(5);
            var race = new VirtualRace(raceName, startTime);

            foreach (var horse in raceHorses)
            {
                // Simple odds calculation based on skill level
                decimal odds = Math.Max(1.1m, 10.0m - (horse.SkillLevel * 0.5m) + (decimal)random.NextDouble());
                odds = Math.Round(odds, 2);
                race.AddParticipant(horse.Id, odds);
            }

            await _raceRepository.AddAsync(race);
            return MapToDto(race);
        }

        public async Task<Guid> SimulateRaceAsync(Guid raceId)
        {
            var race = await _raceRepository.GetByIdAsync(raceId);
            if (race == null) throw new KeyNotFoundException("Race not found.");
            if (race.IsFinished) throw new InvalidOperationException("Race is already finished.");

            // Simulation logic
            var random = new Random();
            var participants = race.Participants.ToList();
            var totalWeight = participants.Sum(p => (1 / (double)p.Odds));
            
            var roll = random.NextDouble() * totalWeight;
            double cumulative = 0;
            Guid winningHorseId = participants.First().HorseId;

            foreach (var p in participants)
            {
                cumulative += (1 / (double)p.Odds);
                if (roll <= cumulative)
                {
                    winningHorseId = p.HorseId;
                    break;
                }
            }

            race.FinishRace(winningHorseId);
            await _raceRepository.UpdateAsync(race);

            return winningHorseId;
        }

        private VirtualRaceDto MapToDto(VirtualRace race)
        {
            return new VirtualRaceDto(
                race.Id,
                race.Name,
                race.StartTime,
                race.IsFinished,
                race.WinningHorseId,
                race.Participants.Select(p => new VirtualRaceParticipantDto(
                    p.Id,
                    p.HorseId,
                    p.Horse?.Name ?? "Unknown",
                    p.Odds
                ))
            );
        }
    }
}