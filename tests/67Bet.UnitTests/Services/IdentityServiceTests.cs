using System;
using System.Threading.Tasks;
using _67Bet.Identity.Application.Services;
using _67Bet.Identity.Domain.Entities;
using _67Bet.Identity.Domain.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace _67Bet.UnitTests.Services;

public class IdentityServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly IdentityService _identityService;

    public IdentityServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _identityService = new IdentityService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_WhenEmailIsUnique()
    {
        // Arrange
        var username = "testuser";
        var email = "test@example.com";
        var password = "Password123!";
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var user = await _identityService.RegisterAsync(username, email, password);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        BCrypt.Net.BCrypt.Verify(password, user.PasswordHash).Should().BeTrue();
        
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new User("existing", email, "hash");
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await _identityService.Invoking(s => s.RegisterAsync("new", email, "password"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Użytkownik o tym adresie e-mail już istnieje.");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTrue_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "CorrectPassword";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User("testuser", email, passwordHash);
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _identityService.AuthenticateAsync(email, password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test@example.com";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword");
        var user = new User("testuser", email, passwordHash);
        
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _identityService.AuthenticateAsync(email, "WrongPassword");

        // Assert
        result.Should().BeFalse();
    }
}
