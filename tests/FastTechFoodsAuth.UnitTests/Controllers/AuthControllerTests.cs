using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Api.Controllers;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.DTOs;
using FluentValidation;
using FluentValidation.Results;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FastTechFoodsAuth.UnitTests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IValidator<LoginRequestDto>> _validatorMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _validatorMock = new Mock<IValidator<LoginRequestDto>>();
            _controller = new AuthController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkResult()
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

            var expectedUserDto = new UserDto
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                Name = registerDto.Name,
                Roles = new List<string> { "Client" }
            };

            _userServiceMock.Setup(s => s.RegisterAsync(registerDto))
                .ReturnsAsync(expectedUserDto);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<ActionResult<UserDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedUserDto);
        }

        [Fact]
        public async Task Register_WithServiceException_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterUserDto
            {
                Email = "existing@example.com",
                Password = "password123",
                Name = "Test User",
                Role = "Client"
            };

            _userServiceMock.Setup(s => s.RegisterAsync(registerDto))
                .ThrowsAsync(new Exception("Email already in use."));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().BeEquivalentTo(new { message = "Email already in use." });
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkResult()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "password123"
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(loginDto, default))
                .ReturnsAsync(validationResult);

            var expectedAuthResult = new AuthResultDto
            {
                Access_Token = "jwt_token",
                RefreshToken = "refresh_token",
                User = new UserDto
                {
                    Id = Guid.NewGuid(),
                    Email = loginDto.EmailOrCpf,
                    Name = "Test User",
                    Roles = new List<string> { "Client" }
                }
            };

            _userServiceMock.Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(expectedAuthResult);

            // Act
            var result = await _controller.Login(loginDto, _validatorMock.Object);

            // Assert
            result.Should().BeOfType<ActionResult<AuthResultDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedAuthResult);
        }

        [Fact]
        public async Task Login_WithValidationErrors_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "",
                Password = ""
            };

            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure("EmailOrCpf", "O campo emailOrCpf é obrigatório."),
                new ValidationFailure("Password", "A senha é obrigatória.")
            });

            _validatorMock.Setup(v => v.ValidateAsync(loginDto, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.Login(loginDto, _validatorMock.Object);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            var errors = badRequestResult!.Value as IEnumerable<string>;
            errors.Should().Contain("O campo emailOrCpf é obrigatório.");
            errors.Should().Contain("A senha é obrigatória.");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "wrongpassword"
            };

            var validationResult = new ValidationResult();
            _validatorMock.Setup(v => v.ValidateAsync(loginDto, default))
                .ReturnsAsync(validationResult);

            _userServiceMock.Setup(s => s.LoginAsync(loginDto))
                .ThrowsAsync(new Exception("Invalid credentials."));

            // Act
            var result = await _controller.Login(loginDto, _validatorMock.Object);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            unauthorizedResult!.Value.Should().BeEquivalentTo(new { message = "Invalid credentials." });
        }

        [Fact]
        public async Task Me_WithValidToken_ShouldReturnUserData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new UserDto
            {
                Id = userId,
                Email = "test@example.com",
                Name = "Test User",
                Roles = new List<string> { "Client" }
            };

            _userServiceMock.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Setup authenticated user context
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString()),
                new Claim("email", "test@example.com"),
                new Claim("name", "Test User")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.Me();

            // Assert
            result.Should().BeOfType<ActionResult<UserDto>>();
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(expectedUser);
        }

        [Fact]
        public async Task Me_WithoutSubClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("email", "test@example.com"),
                new Claim("name", "Test User")
                // Missing 'sub' claim
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.Me();

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Me_WithInvalidGuidInSubClaim_ShouldReturnUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("sub", "invalid-guid"),
                new Claim("email", "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.Me();

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Me_WithNonExistentUser_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(s => s.GetByIdAsync(userId))
                .ReturnsAsync((UserDto?)null);

            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = await _controller.Me();

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public void AdminOnly_WithValidClaims_ShouldReturnOkResult()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString()),
                new Claim("name", "Admin User"),
                new Claim("roles", "Admin")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = _controller.AdminOnly();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var value = okResult!.Value;
            
            value.Should().NotBeNull();
            // Check if the response contains expected properties
            var response = value!.GetType().GetProperty("message")?.GetValue(value);
            response.Should().Be("Acesso autorizado para Admin");
        }

        [Fact]
        public void TokenInfo_WithValidClaims_ShouldReturnClaimsInformation()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim("sub", userId.ToString()),
                new Claim("email", "test@example.com"),
                new Claim("name", "Test User"),
                new Claim("roles", "Client")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Act
            var result = _controller.TokenInfo();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().NotBeNull();
            
            // Verify that the response contains claims information
            var response = okResult.Value;
            var isAuthenticatedProperty = response!.GetType().GetProperty("isAuthenticated");
            isAuthenticatedProperty.Should().NotBeNull();
            
            var claimsProperty = response.GetType().GetProperty("claims");
            claimsProperty.Should().NotBeNull();
        }
    }
}
