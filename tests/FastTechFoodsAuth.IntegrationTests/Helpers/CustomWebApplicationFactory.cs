using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FastTechFoodsAuth.Infra.Context;

namespace FastTechFoodsAuth.IntegrationTests.Helpers
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                
                // Add in-memory configuration for testing
                var testConfig = new Dictionary<string, string?>
                {
                    ["JWT_SECRET"] = "D9sF2k8k3nB8x4P7vL6hA1pC0rS3qW2eX",
                    ["JWT_EXPIRATION_HOURS"] = "24",
                    ["JWT_REFRESH_EXPIRATION_DAYS"] = "7",
                    ["JWT_ISSUER"] = "FastTechFoodsAuth",
                    ["JWT_AUDIENCE"] = "FastTechFoodsAuth",
                    ["ConnectionStrings:Default"] = "InMemory"
                };
                
                config.AddInMemoryCollection(testConfig);
            });

            builder.ConfigureServices(services =>
            {
                // Remove observability services that might cause issues
                var descriptorsToRemove = services.Where(d => 
                    d.ServiceType.FullName?.Contains("HealthCheck") == true ||
                    d.ServiceType.FullName?.Contains("Prometheus") == true ||
                    d.ServiceType.FullName?.Contains("Observability") == true ||
                    d.ServiceType.FullName?.Contains("OpenTelemetry") == true ||
                    d.ServiceType.FullName?.Contains("Serilog") == true).ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

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

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
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
