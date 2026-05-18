using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Application.DTOs;

namespace _67Bet.Betting.Application.Interfaces
{
    public interface IVirtualRacingService
    {
        Task<IEnumerable<VirtualRaceDto>> GetActiveRacesAsync();
        Task<VirtualRaceDto> GenerateRaceAsync();
        Task<Guid> SimulateRaceAsync(Guid raceId);
    }
}