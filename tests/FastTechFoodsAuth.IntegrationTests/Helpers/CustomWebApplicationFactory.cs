using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FastTechFoodsAuth.Infra.Context;

namespace FastTechFoodsAuth.IntegrationTests.Helpers
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.RemoveAll(typeof(ApplicationDbContext));

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid());
                });

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created
                context.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(context);
            });

            builder.UseEnvironment("Testing");
        }

        private static void SeedTestData(ApplicationDbContext context)
        {
            // Add test roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new FastTechFoodsAuth.Domain.Entities.Role { Id = Guid.NewGuid(), Name = "Admin" },
                    new FastTechFoodsAuth.Domain.Entities.Role { Id = Guid.NewGuid(), Name = "Client" },
                    new FastTechFoodsAuth.Domain.Entities.Role { Id = Guid.NewGuid(), Name = "User" }
                );
                context.SaveChanges();
            }
        }
    }
}
