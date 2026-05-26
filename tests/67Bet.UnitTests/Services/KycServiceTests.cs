using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using _67Bet.Identity.Application.Services;
using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;
using _67Bet.Identity.Domain.Enums;

namespace _67Bet.UnitTests.Services
{
    public class KycServiceTests
    {
        private readonly Mock<IKycSessionRepository> _kycSessionRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly KycService _kycService;

        public KycServiceTests()
        {
            _kycSessionRepositoryMock = new Mock<IKycSessionRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _kycService = new KycService(_kycSessionRepositoryMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task GenerateSessionAsync_ShouldCreateNewSessionAndReturnId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _kycSessionRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<KycSession>()))
                .Returns(Task.CompletedTask);

            // Act
            var sessionId = await _kycService.GenerateSessionAsync(userId);

            // Assert
            Assert.NotEqual(Guid.Empty, sessionId);
            _kycSessionRepositoryMock.Verify(repo => repo.AddAsync(It.Is<KycSession>(s => s.Id == sessionId && s.UserId == userId)), Times.Once);
        }

        [Fact]
        public async Task CompleteSessionAsync_ShouldUpdateSessionStatus_WhenSessionExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var session = new KycSession(userId);
            var sessionId = session.Id;

            var user = new User("testuser", "test@test.com", "hash", Role.User);

            _kycSessionRepositoryMock.Setup(repo => repo.GetByIdAsync(sessionId))
                .ReturnsAsync(session);
            _kycSessionRepositoryMock.Setup(repo => repo.UpdateAsync(session))
                .Returns(Task.CompletedTask);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            await _kycService.CompleteSessionAsync(sessionId);

            // Assert
            Assert.Equal(KycSessionStatus.Completed, session.Status);
            Assert.True(user.IsKycVerified);
            _kycSessionRepositoryMock.Verify(repo => repo.UpdateAsync(session), Times.Once);
            _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task CompleteSessionAsync_ShouldDoNothing_WhenSessionDoesNotExist()
        {
            // Arrange
            var sessionId = Guid.NewGuid();

            _kycSessionRepositoryMock.Setup(repo => repo.GetByIdAsync(sessionId))
                .ReturnsAsync((KycSession)null!);

            // Act
            await _kycService.CompleteSessionAsync(sessionId);

            // Assert
            _kycSessionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<KycSession>()), Times.Never);
        }
    }
}
