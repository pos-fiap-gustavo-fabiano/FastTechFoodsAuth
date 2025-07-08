using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FastTechFoodsAuth.Security.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace FastTechFoodsAuth.Security.Extensions;

/// <summary>
/// Extensões para configuração de autenticação JWT
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adiciona autenticação JWT padronizada com todas as configurações de segurança
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <param name="configureOptions">Configurações adicionais opcionais</param>
    /// <returns>Services collection para encadeamento</returns>
    public static IServiceCollection AddFastTechFoodsJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtConfiguration>? configureOptions = null)
    {
        var jwtConfig = new JwtConfiguration();
        configuration.GetSection(JwtConfiguration.SectionName).Bind(jwtConfig);

        var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (!string.IsNullOrEmpty(envSecret))
        {
            jwtConfig.Key = envSecret;
        }

        configureOptions?.Invoke(jwtConfig);

        jwtConfig.Validate();

        services.Configure<JwtConfiguration>(config =>
        {
            config.Key = jwtConfig.Key;
            config.Issuer = jwtConfig.Issuer;
            config.Audience = jwtConfig.Audience;
            config.ExpirationHours = jwtConfig.ExpirationHours;
            config.ClockSkew = jwtConfig.ClockSkew;
        });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                ValidateIssuer = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtConfig.Audience,
                ValidateLifetime = true,
                ClockSkew = jwtConfig.ClockSkew,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                SaveSigninToken = false
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("Falha na autenticação JWT: {Exception} | Path: {Path} | IP: {IP}", 
                        context.Exception.Message, 
                        context.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress);
                    
                    context.HttpContext.Items["JwtError"] = context.Exception.Message;
                    context.HttpContext.Items["JwtErrorType"] = context.Exception.GetType().Name;
                    
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    logger.LogWarning("JWT Challenge: {Error} - {Description} | Path: {Path}", 
                        context.Error, 
                        context.ErrorDescription,
                        context.Request.Path);
                    
                    context.HandleResponse();
                    
                    var errorMessage = GetFriendlyErrorMessage(context.HttpContext.Items["JwtError"]?.ToString());
                    var errorType = context.HttpContext.Items["JwtErrorType"]?.ToString();
                    
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        message = "Não autorizado",
                        error = errorMessage,
                        errorType = errorType,
                        timestamp = DateTime.UtcNow,
                        path = context.Request.Path.Value
                    };

                    return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value ?? "Unknown";
                    logger.LogDebug("Token JWT validado com sucesso para usuário: {UserId}", userId);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
    private static string GetFriendlyErrorMessage(string? technicalError)
    {
        if (string.IsNullOrEmpty(technicalError))
            return "Token inválido ou ausente";

        return technicalError.ToLower() switch
        {
            var msg when msg.Contains("expired") => "Token expirado",
            var msg when msg.Contains("signature") => "Assinatura do token inválida",
            var msg when msg.Contains("issuer") => "Emissor do token inválido",
            var msg when msg.Contains("audience") => "Destinatário do token inválido",
            var msg when msg.Contains("notbefore") => "Token ainda não é válido",
            var msg when msg.Contains("malformed") => "Formato do token inválido",
            _ => "Token inválido ou ausente"
        };
    }
}
