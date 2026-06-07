using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _67Bet.Betting.Domain.Entities.VirtualRacing;

namespace _67Bet.Betting.Domain.Repositories
{
    public interface IVirtualRaceRepository
    {
        Task<IEnumerable<VirtualRace>> GetActiveRacesAsync();
        Task<IEnumerable<VirtualRace>> GetFinishedRacesAsync();
        Task<VirtualRace?> GetByIdAsync(Guid id);
        Task<VirtualRace?> GetByParticipantIdAsync(Guid participantId);
        Task AddAsync(VirtualRace race);
        Task UpdateAsync(VirtualRace race);
    }
}