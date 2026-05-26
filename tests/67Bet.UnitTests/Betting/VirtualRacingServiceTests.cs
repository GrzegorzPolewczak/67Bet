using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using _67Bet.Betting.Application.Services;
using _67Bet.Betting.Domain.Entities.VirtualRacing;
using _67Bet.Betting.Domain.Repositories;

namespace _67Bet.UnitTests.Betting
{
    public class VirtualRacingServiceTests
    {
        private readonly Mock<IVirtualRaceRepository> _mockRaceRepo;
        private readonly Mock<IHorseRepository> _mockHorseRepo;
        private readonly VirtualRacingService _service;

        public VirtualRacingServiceTests()
        {
            _mockRaceRepo = new Mock<IVirtualRaceRepository>();
            _mockHorseRepo = new Mock<IHorseRepository>();
            _service = new VirtualRacingService(_mockRaceRepo.Object, _mockHorseRepo.Object);
        }

        [Fact]
        public async Task GenerateRaceAsync_ThrowsException_WhenNotEnoughHorses()
        {
            // Arrange
            _mockHorseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Horse> { new Horse("Lonely Horse", 5) });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateRaceAsync());
        }

        [Fact]
        public async Task GenerateRaceAsync_CreatesRaceAndParticipants_WhenEnoughHorses()
        {
            // Arrange
            var horses = new List<Horse>
            {
                new Horse("Thunder", 8),
                new Horse("Lightning", 7),
                new Horse("Storm", 6)
            };
            _mockHorseRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(horses);

            // Act
            var result = await _service.GenerateRaceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsFinished);
            Assert.Null(result.WinningHorseId);
            Assert.Equal(3, result.Participants.Count());
            _mockRaceRepo.Verify(r => r.AddAsync(It.IsAny<VirtualRace>()), Times.Once);
        }

        [Fact]
        public async Task SimulateRaceAsync_SetsWinningHorseAndFinishesRace()
        {
            // Arrange
            var race = new VirtualRace("Test Race", DateTime.UtcNow);
            var horse1 = new Horse("H1", 5);
            var horse2 = new Horse("H2", 5);
            race.AddParticipant(horse1.Id, 2.0m);
            race.AddParticipant(horse2.Id, 3.0m);

            _mockRaceRepo.Setup(r => r.GetByIdAsync(race.Id)).ReturnsAsync(race);

            // Act
            var winningId = await _service.SimulateRaceAsync(race.Id);

            // Assert
            Assert.True(race.IsFinished);
            Assert.Equal(winningId, race.WinningHorseId);
            _mockRaceRepo.Verify(r => r.UpdateAsync(race), Times.Once);
        }
    }
}