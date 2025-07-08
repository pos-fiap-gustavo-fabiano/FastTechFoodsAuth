using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Domain.Entities;
using FastTechFoodsAuth.UnitTests.Helpers;
using System.IdentityModel.Tokens.Jwt;

namespace FastTechFoodsAuth.UnitTests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            SetupConfiguration();
            _tokenService = new TokenService(_configurationMock.Object);
        }

        private void SetupConfiguration()
        {
            _configurationMock.Setup(c => c["Jwt:Key"])
                .Returns("D9sF2k8k3nB8x4P7vL6hA1pC0rS3qW2e");
            _configurationMock.Setup(c => c["Jwt:Issuer"])
                .Returns("FastTechFoodsAuth");
            _configurationMock.Setup(c => c["Jwt:Audience"])
                .Returns("FastTechFoodsUsers");
            _configurationMock.Setup(c => c["Jwt:ExpirationHours"])
                .Returns("2");
        }

        [Fact]
        public void GenerateJwtToken_WithValidUser_ShouldReturnValidToken()
        {
            // Arrange
            var user = TestDataBuilder.CreateValidUser();

            // Act
            var token = _tokenService.GenerateJwtToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
            jsonToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == user.Email);
            jsonToken.Claims.Should().Contain(c => c.Type == "name" && c.Value == user.Name);
            jsonToken.Issuer.Should().Be("FastTechFoodsAuth");
            jsonToken.Audiences.Should().Contain("FastTechFoodsUsers");
        }

        [Fact]
        public void GenerateJwtToken_WithUserWithRoles_ShouldIncludeRolesClaim()
        {
            // Arrange
            var user = TestDataBuilder.CreateValidUser();

            // Act
            var token = _tokenService.GenerateJwtToken(user);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.Claims.Should().Contain(c => c.Type == "roles" && c.Value.Contains("Client"));
        }

        [Fact]
        public void GenerateJwtToken_WhenJwtSecretNotConfigured_ShouldThrowException()
        {
            // Arrange
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);
            Environment.SetEnvironmentVariable("JWT_SECRET", null);
            var tokenService = new TokenService(_configurationMock.Object);
            var user = TestDataBuilder.CreateValidUser();

            // Act & Assert
            var act = () => tokenService.GenerateJwtToken(user);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("JWT Secret not configured");
        }

        [Fact]
        public void GenerateJwtToken_WithEnvironmentVariable_ShouldUseEnvironmentSecret()
        {
            // Arrange
            Environment.SetEnvironmentVariable("JWT_SECRET", "environment_secret_key_32_chars");
            var tokenService = new TokenService(_configurationMock.Object);
            var user = TestDataBuilder.CreateValidUser();

            // Act
            var token = tokenService.GenerateJwtToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            // Cleanup
            Environment.SetEnvironmentVariable("JWT_SECRET", null);
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnNonEmptyGuid()
        {
            // Arrange
            var user = TestDataBuilder.CreateValidUser();

            // Act
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            // Assert
            refreshToken.Should().NotBeNullOrEmpty();
            refreshToken.Should().HaveLength(32); // GUID without hyphens
            Guid.TryParse(refreshToken, out _).Should().BeFalse(); // Should be N format (no hyphens)
        }

        [Theory]
        [InlineData("1")]
        [InlineData("24")]
        [InlineData("168")] // 1 week
        public void GenerateJwtToken_WithDifferentExpirationHours_ShouldSetCorrectExpiration(string hours)
        {
            // Arrange
            _configurationMock.Setup(c => c["Jwt:ExpirationHours"]).Returns(hours);
            var tokenService = new TokenService(_configurationMock.Object);
            var user = TestDataBuilder.CreateValidUser();
            var expectedExpiration = DateTime.UtcNow.AddHours(int.Parse(hours));

            // Act
            var token = tokenService.GenerateJwtToken(user);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            jsonToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void GenerateJwtToken_WithUserWithoutRoles_ShouldHandleEmptyRoles()
        {
            // Arrange
            var user = TestDataBuilder.CreateValidUser();
            user.UserRoles = new List<UserRole>(); // Empty roles

            // Act
            var token = _tokenService.GenerateJwtToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            var rolesClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "roles");
            rolesClaim?.Value.Should().BeEmpty();
        }
    }
}
