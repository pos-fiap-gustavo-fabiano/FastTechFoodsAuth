using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FastTechFoodsAuth.Infra.Context;
using FastTechFoodsAuth.Domain.Entities;

namespace FastTechFoodsAuth.IntegrationTests.Helpers
{
    public static class TestDataHelper
    {
        public static async Task<User> CreateTestUserAsync(IServiceScope scope, string email = "test@example.com", string role = "Client")
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (userRole == null)
            {
                userRole = new Role { Id = Guid.NewGuid(), Name = role };
                context.Roles.Add(userRole);
                await context.SaveChangesAsync();
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = "Test User",
                CPF = "12345678901",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                CreatedAt = DateTime.UtcNow,
                UserRoles = new List<UserRole>()
            };

            var userRoleAssociation = new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id,
                User = user,
                Role = userRole
            };

            user.UserRoles.Add(userRoleAssociation);
            
            context.Users.Add(user);
            context.UserRoles.Add(userRoleAssociation);
            await context.SaveChangesAsync();

            return user;
        }

        public static async Task ClearDatabaseAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            context.UserRoles.RemoveRange(context.UserRoles);
            context.Users.RemoveRange(context.Users);
            context.Roles.RemoveRange(context.Roles);
            
            await context.SaveChangesAsync();
        }
    }
}
