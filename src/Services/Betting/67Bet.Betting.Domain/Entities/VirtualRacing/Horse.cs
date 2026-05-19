using System;
using _67Bet.Shared.Kernel;

namespace _67Bet.Betting.Domain.Entities.VirtualRacing
{
    public class Horse : BaseEntity, IAggregateRoot
    {
        public string Name { get; private set; }
        public int SkillLevel { get; private set; }

        protected Horse() { } // EF Core

        public Horse(string name, int skillLevel)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            Name = name;
            SkillLevel = skillLevel;
        }
    }
}