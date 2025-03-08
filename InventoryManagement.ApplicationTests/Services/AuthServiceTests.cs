
using FluentAssertions;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Persistence.Interfaces;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Common.Responses;
using InventoryManagement.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace InventoryManagement.ApplicationTests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<IUserRepository> _mockUserRepository;

        private readonly Mock<IJwtService> _mockJwtService;

        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        private readonly AuthService _authService;


        public AuthServiceTests()
        {
            //set up mocks
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            //create instance of AuthService with mocked dependencies
            _authService = new AuthService(_mockLogger.Object, _mockUserRepository.Object, _mockJwtService.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnAuthResponse()
        {
            // Arrange
            var authRequest = new Application.DTOs.AuthRequestDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User("testuser", BCrypt.Net.BCrypt.HashPassword("password123"), "test@example.com", "User");
            user.Id = 1;

            string token = "test.jwt.token";

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(authRequest.Email))
                .ReturnsAsync(user);
            _mockJwtService.Setup(service => service.GenerateToken(user))
                .Returns(token);

            // Act
            var result = await _authService.LoginAsync(authRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(token);
            result.User.Should().NotBeNull();
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be(user.Email);
            result.User.Username.Should().Be(user.Username);
            result.User.Role.Should().Be(user.Role);

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(authRequest.Email), Times.Once);
            _mockJwtService.Verify(service => service.GenerateToken(user), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var authRequest = new AuthRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(authRequest.Email))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.LoginAsync(authRequest));

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(authRequest.Email), Times.Once);
            _mockJwtService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_ShouldThrowBadRequestException()
        {
            // Arrange
            var authRequest = new AuthRequestDto
            {
                Email = "test@example.com",
                Password = "wrongpassword"
            };

            var user = new User("testuser", BCrypt.Net.BCrypt.HashPassword("password123"), "test@example.com", "User");
            user.Id = 1;

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(authRequest.Email))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _authService.LoginAsync(authRequest));

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(authRequest.Email), Times.Once);
            _mockJwtService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserDoesNotExist_ShouldCreateUserAndReturnAuthResponse()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "password123",
                Role = "User"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync((User)null);
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _mockUserRepository.Setup(repo => repo.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _mockJwtService.Setup(service => service.GenerateToken(It.IsAny<User>()))
                .Returns("test.jwt.token");

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("test.jwt.token");
            result.User.Should().NotBeNull();
            result.User.Username.Should().Be(registerRequest.Username);
            result.User.Email.Should().Be(registerRequest.Email);
            result.User.Role.Should().Be(registerRequest.Role);

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(registerRequest.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
            _mockJwtService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Once);
        }


        [Fact]
        public async Task RegisterAsync_WhenUserAlreadyExists_ShouldThrowBadRequestException()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "password123",
                Role = "User"
            };

            var existingUser = new User("existinguser", "hashedpassword", "existing@example.com", "User");

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(registerRequest.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _authService.RegisterAsync(registerRequest));

            _mockUserRepository.Verify(repo => repo.GetByEmailAsync(registerRequest.Email), Times.Once);
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _mockJwtService.Verify(service => service.GenerateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserIsAuthenticatedAndExists_ShouldReturnUserDto()
        {
            // Arrange
            string userId = "1";
            var httpContext = new DefaultHttpContext();
            httpContext.Items["UserId"] = userId;

            var user = new User("currentuser", "hashedpassword", "current@example.com", "User");
            user.Id = 1;

            _mockHttpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(httpContext);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(int.Parse(userId)))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.GetCurrentUserAsync();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Username.Should().Be(user.Username);
            result.Email.Should().Be(user.Email);
            result.Role.Should().Be(user.Role);

            _mockUserRepository.Verify(repo => repo.GetByIdAsync(int.Parse(userId)), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserIsNotAuthenticated_ShouldThrowUnauthorizedException()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            // Not setting UserId in Items

            _mockHttpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(httpContext);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _authService.GetCurrentUserAsync());

            _mockUserRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUserAsync_WhenUserDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            string userId = "999";
            var httpContext = new DefaultHttpContext();
            httpContext.Items["UserId"] = userId;

            _mockHttpContextAccessor.Setup(accessor => accessor.HttpContext)
                .Returns(httpContext);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(int.Parse(userId)))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _authService.GetCurrentUserAsync());

            _mockUserRepository.Verify(repo => repo.GetByIdAsync(int.Parse(userId)), Times.Once);
        }

    }
}