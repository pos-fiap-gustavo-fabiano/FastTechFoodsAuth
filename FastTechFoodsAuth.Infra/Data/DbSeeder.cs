using FastTechFoodsAuth.Domain.Entities;
using FastTechFoodsAuth.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace FastTechFoodsAuth.Infra.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Garante que o banco de dados está criado
            await context.Database.MigrateAsync();

            // Seed Roles
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Id = Guid.NewGuid(), Name = "Admin" },
                    new Role { Id = Guid.NewGuid(), Name = "Manager" },
                    new Role { Id = Guid.NewGuid(), Name = "Employee" },
                    new Role { Id = Guid.NewGuid(), Name = "Client" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

                // ATENÇÃO: A senha deve ser gerada por hash seguro (ex: BCrypt)
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Administrador",
                    Email = "admin@fasttechfoods.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Use um hash real em produção!
                    CreatedAt = DateTime.UtcNow,
                    UserRoles = new List<UserRole>()
                };

                if (adminRole != null)
                {
                    admin.UserRoles.Add(new UserRole
                    {
                        UserId = admin.Id,
                        RoleId = adminRole.Id
                    });
                }

                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}
