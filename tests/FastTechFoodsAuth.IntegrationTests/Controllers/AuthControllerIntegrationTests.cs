using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.IntegrationTests.Helpers;
using System.Net;

namespace FastTechFoodsAuth.IntegrationTests.Controllers
{
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnCreatedUser()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = $"test_{Guid.NewGuid()}@example.com",
                Password = "password123",
                Name = "Integration Test User",
                CPF = "12345678901",
                Role = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var userDto = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(registerRequest.Email);
            userDto.Name.Should().Be(registerRequest.Name);
            userDto.Id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var email = $"duplicate_{Guid.NewGuid()}@example.com";
            
            var registerRequest1 = new RegisterUserDto
            {
                Email = email,
                Password = "password123",
                Name = "First User",
                CPF = "12345678901",
                Role = "User"
            };

            var registerRequest2 = new RegisterUserDto
            {
                Email = email, // Same email
                Password = "password456",
                Name = "Second User",
                CPF = "12345678902",
                Role = "User"
            };

            // Act
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest1);
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest2);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnAuthResult()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var testUser = await TestDataHelper.CreateTestUserAsync(scope, "login_test@example.com", "Client");

            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "login_test@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResultDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            authResult.Should().NotBeNull();
            authResult!.Token.Should().NotBeNullOrEmpty();
            authResult.RefreshToken.Should().NotBeNullOrEmpty();
            authResult.User.Should().NotBeNull();
            authResult.User.Email.Should().Be(testUser.Email);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await TestDataHelper.CreateTestUserAsync(scope, "invalid_test@example.com", "Client");

            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "invalid_test@example.com",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithCPF_ShouldReturnAuthResult()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var testUser = await TestDataHelper.CreateTestUserAsync(scope, "cpf_test@example.com", "Client");

            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "12345678901", // Using CPF instead of email
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Me_WithValidToken_ShouldReturnUserInfo()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var testUser = await TestDataHelper.CreateTestUserAsync(scope, "me_test@example.com", "Client");

            // First, login to get token
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "me_test@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResultDto>(loginContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Add Authorization header
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var userDto = JsonSerializer.Deserialize<UserDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            userDto.Should().NotBeNull();
            userDto!.Email.Should().Be(testUser.Email);
            userDto.Name.Should().Be(testUser.Name);
        }

        [Fact]
        public async Task Me_WithoutToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Me_WithInvalidToken_ShouldReturnUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid_token");

            // Act
            var response = await _client.GetAsync("/api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TokenInfo_WithValidToken_ShouldReturnTokenDetails()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await TestDataHelper.CreateTestUserAsync(scope, "token_info@example.com", "Client");

            // Login to get token
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "token_info@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResultDto>(loginContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/auth/token-info");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("isAuthenticated");
            responseContent.Should().Contain("claims");
            responseContent.Should().Contain("sub");
        }

        [Fact]
        public async Task AdminOnly_WithAdminToken_ShouldReturnOk()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await TestDataHelper.CreateTestUserAsync(scope, "admin@example.com", "Admin");

            // Login to get token
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "admin@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResultDto>(loginContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/auth/admin-only");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AdminOnly_WithClientToken_ShouldReturnForbidden()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            await TestDataHelper.CreateTestUserAsync(scope, "client@example.com", "Client");

            // Login to get token
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "client@example.com",
                Password = "password123"
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var authResult = JsonSerializer.Deserialize<AuthResultDto>(loginContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

            // Act
            var response = await _client.GetAsync("/api/auth/admin-only");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
