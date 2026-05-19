using System;
using System.Collections.Generic;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.VirtualRacing
{
    public class VirtualRace : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public DateTime StartTime { get; private set; }
        public bool IsFinished { get; private set; }
        public Guid? WinningHorseId { get; private set; }

        private readonly List<VirtualRaceParticipant> _participants = new();
        public IReadOnlyCollection<VirtualRaceParticipant> Participants => _participants.AsReadOnly();

        protected VirtualRace() { } // EF Core

        public VirtualRace(string name, DateTime startTime)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Name = name;
            StartTime = startTime;
            IsFinished = false;
        }

        public void AddParticipant(Guid horseId, decimal odds)
        {
            var participant = new VirtualRaceParticipant(Id, horseId, odds);
            _participants.Add(participant);
        }

        public void FinishRace(Guid winningHorseId)
        {
            IsFinished = true;
            WinningHorseId = winningHorseId;
        }
    }
}