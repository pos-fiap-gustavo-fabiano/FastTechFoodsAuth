using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Application.Validators;
using FastTechFoodsAuth.Application.DTOs;

namespace FastTechFoodsAuth.UnitTests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator;

        public LoginRequestValidatorTests()
        {
            _validator = new LoginRequestValidator();
        }

        [Fact]
        public void Validate_WithValidEmailOrCpfAndPassword_ShouldBeValid()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyEmailOrCpf_ShouldBeInvalid(string emailOrCpf)
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = emailOrCpf,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo emailOrCpf é obrigatório.");
        }

        [Fact]
        public void Validate_WithEmailOrCpfTooLong_ShouldBeInvalid()
        {
            // Arrange
            var longEmailOrCpf = new string('a', 151) + "@example.com";
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = longEmailOrCpf,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo Email ou CPF deve ter no máximo 150 caracteres.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyPassword_ShouldBeInvalid(string password)
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = password
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha é obrigatória.");
        }

        [Fact]
        public void Validate_WithPasswordTooShort_ShouldBeInvalid()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = "12345" // 5 characters, minimum is 6
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no mínimo 6 caracteres.");
        }

        [Fact]
        public void Validate_WithPasswordTooLong_ShouldBeInvalid()
        {
            // Arrange
            var longPassword = new string('a', 101); // 101 characters, maximum is 100
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = longPassword
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no máximo 100 caracteres.");
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.co.uk")]
        [InlineData("12345678901")]
        [InlineData("123.456.789-01")]
        public void Validate_WithValidEmailOrCpfFormats_ShouldBeValid(string emailOrCpf)
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = emailOrCpf,
                Password = "password123"
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("password")]
        [InlineData("123456")]
        [InlineData("abcdef")]
        [InlineData("P@ssw0rd")]
        public void Validate_WithValidPasswordLengths_ShouldBeValid(string password)
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "test@example.com",
                Password = password
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                EmailOrCpf = "", // Empty
                Password = "123" // Too short
            };

            // Act
            var result = _validator.Validate(loginRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(2);
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo emailOrCpf é obrigatório.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no mínimo 6 caracteres.");
        }
    }
}
