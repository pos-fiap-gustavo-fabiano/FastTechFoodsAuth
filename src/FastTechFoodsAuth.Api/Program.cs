using FastTechFoods.Observability;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.Mapping;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Infra.Context;
using FastTechFoodsAuth.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FastTechFoodsAuth.Application.Validators;
using FastTechFoodsAuth.Api.Middleware;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using FastTechFoodsAuth.Api.HealthChecks;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FastTechFoodsAuth.Security.Extensions;


// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build())
    .WriteTo.Console()
    .WriteTo.File("logs/fasttechfoodsauth-.log", 
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FastTechFoodsAuth")
    .CreateLogger();

try
{
    Log.Information("Iniciando FastTechFoodsAuth API");

    var builder = WebApplication.CreateBuilder(args);
    
    // Usar Serilog como provider de logging
    builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✨ Configuração simplificada do Swagger com JWT usando a biblioteca
builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsAuth API", "v1", "API de autenticação para o sistema FastTechFoods");

// ✨ Configuração simplificada da autenticação JWT usando a biblioteca
builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database-connectivity", tags: new[] { "database" })
    .AddDbContextCheck<ApplicationDbContext>("database-ef", tags: new[] { "database" });

//Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

//Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

//Validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
    ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); ;

builder.Services.AddFastTechFoodsObservability(
    serviceName: "FastTechFoodsAuth.Api",
    serviceVersion: "1.0.0",
    otlpEndpoint: "http://4.198.128.197:4317"
);

var app = builder.Build();

// Health Checks endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("database"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Não executa checks, apenas confirma que a app está rodando
});

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//    await DbSeeder.SeedAsync(dbContext);
//}
app.UseSwagger();
app.UseSwaggerUI();


// Middleware de tratamento global de erros
app.UseMiddleware<GlobalExceptionMiddleware>();

// ✨ Middleware de auditoria de segurança (opcional)
app.UseFastTechFoodsSecurityAudit();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("FastTechFoodsAuth API iniciada com sucesso");
app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Erro fatal ao iniciar a aplicação");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public and partial so integration tests can reference it
public partial class Program { }
