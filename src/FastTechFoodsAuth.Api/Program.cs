using FastTechFoods.Observability;
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
   

    builder.Services.AddFastTechFoodsObservabilityAndHealthChecks<ApplicationDbContext>(builder.Configuration);

    var app = builder.Build();

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
    app.UseFastTechFoodsHealthChecksUI();
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

public partial class Program { }
