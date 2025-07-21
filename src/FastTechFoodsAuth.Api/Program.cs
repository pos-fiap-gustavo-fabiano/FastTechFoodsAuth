using FastTechFoodsAuth.Api.Config;
using FastTechFoodsAuth.Api.Middleware;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.Mapping;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Application.Validators;
using FastTechFoodsAuth.Infra.Context;
using FastTechFoodsAuth.Infra.Repositories;
using FastTechFoodsAuth.Security.Extensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    Log.Information("Iniciando FastTechFoodsAuth API");
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddFastTechFoodsSwaggerWithJwt("FastTechFoodsAuth API", "v1", "API de autenticação para o sistema FastTechFoods");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });
    // ✨ Configuração simplificada da autenticação JWT usando a biblioteca
    builder.Services.AddFastTechFoodsJwtAuthentication(builder.Configuration);
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
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


    ObservabilityConfig.AddObservability(builder);

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
    // Middleware de tratamento global de erros
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseFastTechFoodsSecurityAudit();
    app.UseAuthentication();
    app.UseAuthorization();
    ObservabilityConfig.UseObservability(app);
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
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

public partial class Program { }
