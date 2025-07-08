using FastTechFoodsAuth.Domain.Entities;

namespace FastTechFoodsAuth.UnitTests.Helpers
{
    public static class TestDataBuilder
    {
        public static User CreateValidUser(string? email = null, string? name = null, string? cpf = null)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email ?? "test@example.com",
                Name = name ?? "Test User",
                CPF = cpf ?? "12345678901",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>
                {
                    new UserRole
                    {
                        UserId = Guid.NewGuid(),
                        RoleId = Guid.NewGuid(),
                        Role = new Role
                        {
                            Id = Guid.NewGuid(),
                            Name = "Client",
                            UserRoles = new List<UserRole>()
                        }
                    }
                }
            };
        }

        public static Role CreateValidRole(string? name = null)
        {
            return new Role
            {
                Id = Guid.NewGuid(),
                Name = name ?? "Client",
                UserRoles = new List<UserRole>()
            };
        }

        public static UserRole CreateUserRole(Guid? userId = null, Guid? roleId = null)
        {
            return new UserRole
            {
                UserId = userId ?? Guid.NewGuid(),
                RoleId = roleId ?? Guid.NewGuid(),
                User = CreateValidUser(),
                Role = CreateValidRole()
            };
        }
    }
}
