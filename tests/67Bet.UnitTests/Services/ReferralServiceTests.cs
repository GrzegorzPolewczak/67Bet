using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _67Bet.Wallet.Application.Services;
using _67Bet.Wallet.Application.Interfaces;
using _67Bet.Wallet.Domain.Entities;
using _67Bet.Wallet.Domain.Repositories;
using Moq;
using Xunit;
using FluentAssertions;

namespace _67Bet.UnitTests.Services
{
    public class ReferralServiceTests
    {
        private readonly Mock<IReferralCodeRepository> _referralRepoMock;
        private readonly Mock<IPromoCodeRepository> _promoRepoMock;
        private readonly Mock<IUserCodeUsageRepository> _usageRepoMock;
        private readonly Mock<IWalletService> _walletServiceMock;
        private readonly ReferralService _referralService;

        public ReferralServiceTests()
        {
            _referralRepoMock = new Mock<IReferralCodeRepository>();
            _promoRepoMock = new Mock<IPromoCodeRepository>();
            _usageRepoMock = new Mock<IUserCodeUsageRepository>();
            _walletServiceMock = new Mock<IWalletService>();

            _referralService = new ReferralService(
                _referralRepoMock.Object,
                _promoRepoMock.Object,
                _usageRepoMock.Object,
                _walletServiceMock.Object);
        }

        [Fact]
        public async Task ApplyCodeAsync_ShouldApplyPromoCode_WhenValidAndNotUsed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var code = "PROMO100";
            var promoCode = new PromoCode(code, 100m);

            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(promoCode);
            _usageRepoMock.Setup(r => r.HasUsedCodeAsync(userId, promoCode.Id)).ReturnsAsync(false);

            // Act
            await _referralService.ApplyCodeAsync(userId, code);

            // Assert
            _walletServiceMock.Verify(s => s.DepositFreebetAsync(userId, 100m), Times.Once);
            _usageRepoMock.Verify(r => r.AddAsync(It.IsAny<UserCodeUsage>()), Times.Once);
        }

        [Fact]
        public async Task ApplyCodeAsync_ShouldThrow_WhenPromoCodeAlreadyUsed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var code = "PROMO100";
            var promoCode = new PromoCode(code, 100m);

            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(promoCode);
            _usageRepoMock.Setup(r => r.HasUsedCodeAsync(userId, promoCode.Id)).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _referralService.ApplyCodeAsync(userId, code));
        }

        [Fact]
        public async Task ApplyCodeAsync_ShouldApplyReferralCode_WhenValidAndFirstTime()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var code = "FRIEND123";
            var creatorCode = new ReferralCode(creatorId, code);

            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync((PromoCode)null);
            _referralRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(creatorCode);
            _usageRepoMock.Setup(r => r.HasUsedAnyReferralAsync(userId)).ReturnsAsync(false);

            // Act
            await _referralService.ApplyCodeAsync(userId, code);

            // Assert
            _walletServiceMock.Verify(s => s.DepositFreebetAsync(userId, 20.00m), Times.Once);
            _usageRepoMock.Verify(r => r.AddAsync(It.Is<UserCodeUsage>(u => u.IsReferral)), Times.Once);
            _referralRepoMock.Verify(r => r.UpdateAsync(It.Is<ReferralCode>(rc => rc.UsageCount == 1)), Times.Once);
        }

        [Fact]
        public async Task ApplyCodeAsync_ShouldAwardCreator_WhenMilestoneReached()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var code = "STAR";
            var creatorCode = new ReferralCode(creatorId, code);

            // Set usage to 4, so the next one (5) hits the milestone
            typeof(ReferralCode).GetProperty("UsageCount").SetValue(creatorCode, 4);

            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync((PromoCode)null);
            _referralRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(creatorCode);
            _usageRepoMock.Setup(r => r.HasUsedAnyReferralAsync(userId)).ReturnsAsync(false);

            // Act
            await _referralService.ApplyCodeAsync(userId, code);

            // Assert
            creatorCode.UsageCount.Should().Be(5);
            _walletServiceMock.Verify(s => s.DepositFreebetAsync(creatorId, 50m), Times.Once);
        }

        [Fact]
        public async Task DeactivatePromoCodeAsync_ShouldDeactivate_WhenExists()
        {
            // Arrange
            var code = "TESTPROMO";
            var promo = new PromoCode(code, 50m);
            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(promo);

            // Act
            await _referralService.DeactivatePromoCodeAsync(code);

            // Assert
            promo.IsActive.Should().BeFalse();
            _promoRepoMock.Verify(r => r.UpdateAsync(promo), Times.Once);
        }

        [Fact]
        public async Task ActivatePromoCodeAsync_ShouldActivate_WhenExists()
        {
            // Arrange
            var code = "TESTPROMO";
            var promo = new PromoCode(code, 50m);
            promo.Deactivate();
            _promoRepoMock.Setup(r => r.GetByCodeAsync(code)).ReturnsAsync(promo);

            // Act
            await _referralService.ActivatePromoCodeAsync(code);

            // Assert
            promo.IsActive.Should().BeTrue();
            _promoRepoMock.Verify(r => r.UpdateAsync(promo), Times.Once);
        }

        [Fact]
        public async Task GetAllPromoCodesAsync_ShouldReturnAllCodes()
        {
            // Arrange
            var promos = new List<PromoCode>
            {
                new PromoCode("PROMO1", 10m),
                new PromoCode("PROMO2", 20m)
            };
            _promoRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(promos);

            // Act
            var result = await _referralService.GetAllPromoCodesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Code.Should().Be("PROMO1");
        }

        [Fact]
        public async Task GetReferralStatusAsync_ShouldReturnUsedCode_WhenUserHasActivatedReferral()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var creatorId = Guid.NewGuid();
            var referralCodeId = Guid.NewGuid();
            var usedCode = "FRIEND123";

            var myReferralCode = new ReferralCode(userId, "MYCODE");
            var creatorCode = new ReferralCode(creatorId, usedCode);
            typeof(ReferralCode).GetProperty("Id")?.SetValue(creatorCode, referralCodeId);

            var codeUsage = new UserCodeUsage(userId, referralCodeId, true);

            _referralRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(myReferralCode);
            _usageRepoMock.Setup(r => r.HasUsedAnyReferralAsync(userId)).ReturnsAsync(true);
            _usageRepoMock.Setup(r => r.GetUsedReferralAsync(userId)).ReturnsAsync(codeUsage);
            _referralRepoMock.Setup(r => r.GetByIdAsync(referralCodeId)).ReturnsAsync(creatorCode);

            // Act
            var result = await _referralService.GetReferralStatusAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.MyCode.Should().Be("MYCODE");
            result.HasUsedReferral.Should().BeTrue();
            result.UsedReferralCode.Should().Be("FRIEND123");
        }
    }
}
