using System;
using FastTechFoods.Observability;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Application.Mapping;
using FastTechFoodsAuth.Application.Services;
using FastTechFoodsAuth.Infra.Context;
using FastTechFoodsAuth.Infra.Data;
using FastTechFoodsAuth.Infra.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

//Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();

//Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

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
//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//    await DbSeeder.SeedAsync(dbContext);
//}
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
