using Xunit;
using FluentAssertions;
using FastTechFoodsAuth.Application.Validators;
using FastTechFoodsAuth.Application.DTOs;

namespace FastTechFoodsAuth.UnitTests.Validators
{
    public class RegisterUserValidatorTests
    {
        private readonly RegisterUserValidator _validator;

        public RegisterUserValidatorTests()
        {
            _validator = new RegisterUserValidator();
        }

        [Fact]
        public void Validate_WithValidData_ShouldBeValid()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyEmail_ShouldBeInvalid(string email)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = email,
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo email é obrigatório.");
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("test@")]
        [InlineData("@example.com")]
        [InlineData("test.example.com")]
        public void Validate_WithInvalidEmail_ShouldBeInvalid(string email)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = email,
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo email deve ser um endereço de email válido.");
        }

        [Fact]
        public void Validate_WithEmailTooLong_ShouldBeInvalid()
        {
            // Arrange
            var longEmail = new string('a', 140) + "@example.com"; // More than 150 chars
            var registerRequest = new RegisterUserDto
            {
                Email = longEmail,
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo email deve ter no máximo 150 caracteres.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyCPF_ShouldBeInvalid(string cpf)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = cpf,
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo CPF é obrigatório.");
        }

        [Theory]
        [InlineData("123456789")]    // Too short
        [InlineData("123456789012")] // Too long
        [InlineData("1234567890a")]  // Contains letter
        [InlineData("123.456.789-01")] // With formatting
        public void Validate_WithInvalidCPF_ShouldBeInvalid(string cpf)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = cpf,
                Password = "password123",
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo CPF deve conter exatamente 11 dígitos numéricos.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyPassword_ShouldBeInvalid(string password)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = password,
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha é obrigatória.");
        }

        [Fact]
        public void Validate_WithPasswordTooShort_ShouldBeInvalid()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "12345", // 5 characters, minimum is 6
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no mínimo 6 caracteres.");
        }

        [Fact]
        public void Validate_WithPasswordTooLong_ShouldBeInvalid()
        {
            // Arrange
            var longPassword = new string('a', 101); // 101 characters, maximum is 100
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = longPassword,
                Name = "Test User",
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no máximo 100 caracteres.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyName_ShouldBeInvalid(string name)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = name,
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O nome é obrigatório.");
        }

        [Fact]
        public void Validate_WithNameTooLong_ShouldBeInvalid()
        {
            // Arrange
            var longName = new string('a', 101); // 101 characters, maximum is 100
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = longName,
                Role = "Admin"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O nome deve ter no máximo 100 caracteres.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Validate_WithEmptyRole_ShouldBeInvalid(string role)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = role
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O papel é obrigatório.");
        }

        [Theory]
        [InlineData("InvalidRole")]
        [InlineData("client")]
        [InlineData("ADMIN")]
        [InlineData("Manager")]
        public void Validate_WithInvalidRole_ShouldBeInvalid(string role)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = role
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "O papel deve ser 'Admin' ou 'User'.");
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        public void Validate_WithValidRoles_ShouldBeValid(string role)
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "test@example.com",
                CPF = "12345678901",
                Password = "password123",
                Name = "Test User",
                Role = role
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
        {
            // Arrange
            var registerRequest = new RegisterUserDto
            {
                Email = "invalid-email",
                CPF = "123", // Too short
                Password = "12", // Too short
                Name = "", // Empty
                Role = "InvalidRole"
            };

            // Act
            var result = _validator.Validate(registerRequest);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(5);
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo email deve ser um endereço de email válido.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "O campo CPF deve conter exatamente 11 dígitos numéricos.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "A senha deve ter no mínimo 6 caracteres.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "O nome é obrigatório.");
            result.Errors.Should().Contain(e => e.ErrorMessage == "O papel deve ser 'Admin' ou 'User'.");
        }
    }
}
