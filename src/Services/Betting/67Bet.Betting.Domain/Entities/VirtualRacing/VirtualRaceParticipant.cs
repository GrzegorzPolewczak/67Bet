using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.VirtualRacing
{
    public class VirtualRaceParticipant : BaseEntity
    {
        public Guid RaceId { get; private set; }
        public Guid HorseId { get; private set; }
        public decimal Odds { get; private set; }

        public VirtualRace Race { get; private set; }
        public Horse Horse { get; private set; }

        protected VirtualRaceParticipant() { } // EF Core

        public VirtualRaceParticipant(Guid raceId, Guid horseId, decimal odds)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            RaceId = raceId;
            HorseId = horseId;
            Odds = odds;
        }
    }
}