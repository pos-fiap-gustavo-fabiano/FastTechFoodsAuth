using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Domain.Entities;
using FastTechFoodsAuth.UnitTests.Helpers;

namespace FastTechFoodsAuth.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly IMapper _mapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _mapper = AutoMapperHelper.CreateMapper();
            
            _userService = new UserService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _mapper,
                _tokenServiceMock.Object);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldCreateUser()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "test@example.com",
                Password = "password123",
                Name = "Test User",
                CPF = "12345678901",
                Role = "Client"
            };

            var role = TestDataBuilder.CreateValidRole("Client");
            
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);
            _roleRepositoryMock.Setup(r => r.GetByNameAsync("Client"))
                .ReturnsAsync(role);
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(registerDto.Email);
            result.Name.Should().Be(registerDto.Name);
            
            _userRepositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => 
                u.Email == registerDto.Email && 
                u.Name == registerDto.Name &&
                u.PasswordHash != null)), Times.Once);
            _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "existing@example.com",
                Password = "password123",
                Name = "Test User",
                Role = "Client"
            };

            var existingUser = TestDataBuilder.CreateValidUser(registerDto.Email);
            
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync(existingUser);

            // Act & Assert
            var act = async () => await _userService.RegisterAsync(registerDto);
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Email already in use.");
        }

        [Fact]
        public async Task RegisterAsync_WithInvalidRole_ShouldThrowException()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "test@example.com",
                Password = "password123",
                Name = "Test User",
                Role = "InvalidRole"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);
            _roleRepositoryMock.Setup(r => r.GetByNameAsync("InvalidRole"))
                .ReturnsAsync((Role?)null);

            // Act & Assert
            var act = async () => await _userService.RegisterAsync(registerDto);
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Role not found.");
        }

        [Fact]
        public async Task LoginAsync_WithValidEmailAndPassword_ShouldReturnAuthResult()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "password123"
            };

            var user = TestDataBuilder.CreateValidUser(loginDto.EmailOrCpf);
            var expectedToken = "jwt_token";
            var expectedRefreshToken = "refresh_token";

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.EmailOrCpf))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateJwtToken(user))
                .Returns(expectedToken);
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken(user))
                .Returns(expectedRefreshToken);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken);
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task LoginAsync_WithValidCpfAndPassword_ShouldReturnAuthResult()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "12345678901",
                Password = "password123"
            };

            var user = TestDataBuilder.CreateValidUser(cpf: loginDto.EmailOrCpf);
            var expectedToken = "jwt_token";
            var expectedRefreshToken = "refresh_token";

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.EmailOrCpf))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(r => r.GetByCpfAsync(loginDto.EmailOrCpf))
                .ReturnsAsync(user);
            _tokenServiceMock.Setup(t => t.GenerateJwtToken(user))
                .Returns(expectedToken);
            _tokenServiceMock.Setup(t => t.GenerateRefreshToken(user))
                .Returns(expectedRefreshToken);

            // Act
            var result = await _userService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "wrongpassword"
            };

            var user = TestDataBuilder.CreateValidUser(loginDto.EmailOrCpf);

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.EmailOrCpf))
                .ReturnsAsync(user);

            // Act & Assert
            var act = async () => await _userService.LoginAsync(loginDto);
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid credentials.");
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ShouldThrowException()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "nonexistent@example.com",
                Password = "password123"
            };

            _userRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.EmailOrCpf))
                .ReturnsAsync((User?)null);
            _userRepositoryMock.Setup(r => r.GetByCpfAsync(loginDto.EmailOrCpf))
                .ReturnsAsync((User?)null);

            // Act & Assert
            var act = async () => await _userService.LoginAsync(loginDto);
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid credentials.");
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingUser_ShouldReturnUserDto()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = TestDataBuilder.CreateValidUser();
            user.Id = userId;

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(userId);
            result.Email.Should().Be(user.Email);
            result.Name.Should().Be(user.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentUser_ShouldReturnNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task RegisterAsync_WithNullOrEmptyRole_ShouldUseDefaultClientRole(string? role)
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "test@example.com",
                Password = "password123",
                Name = "Test User",
                Role = role
            };

            var clientRole = TestDataBuilder.CreateValidRole("Client");
            
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(registerDto.Email))
                .ReturnsAsync((User?)null);
            _roleRepositoryMock.Setup(r => r.GetByNameAsync("Client"))
                .ReturnsAsync(clientRole);
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            _roleRepositoryMock.Verify(r => r.GetByNameAsync("Client"), Times.Once);
        }
    }
}
