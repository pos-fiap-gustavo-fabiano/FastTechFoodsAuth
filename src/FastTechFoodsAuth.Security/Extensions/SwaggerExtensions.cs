using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FastTechFoodsAuth.Security.Extensions;

public static class SwaggerExtensions
{
    /// <summary>
    /// Adiciona configuração padrão do Swagger com suporte a JWT Bearer token
    /// </summary>
    /// <param name="services">Collection de serviços</param>
    /// <param name="apiTitle">Título da API</param>
    /// <param name="apiVersion">Versão da API</param>
    /// <param name="apiDescription">Descrição da API</param>
    /// <returns>Services collection para encadeamento</returns>
    public static IServiceCollection AddFastTechFoodsSwaggerWithJwt(
        this IServiceCollection services,
        string apiTitle,
        string apiVersion = "v1",
        string? apiDescription = null)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(apiVersion, new OpenApiInfo 
            { 
                Title = apiTitle, 
                Version = apiVersion,
                Description = apiDescription ?? $"API {apiTitle} com autenticação JWT integrada"
            });
            
            // Configuração do JWT Bearer Token
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando o esquema Bearer. " +
                            "Insira 'Bearer' [espaço] e então seu token. " +
                            "Exemplo: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new string[] {}
                }
            });

            // Adiciona comentários XML se disponíveis
            try
            {
                var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            }
            catch
            {
                // Ignora erros de XML comments se não existir
            }
        });

        return services;
    }
}
