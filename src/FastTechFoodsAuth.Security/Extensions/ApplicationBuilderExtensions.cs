using Microsoft.AspNetCore.Builder;
using FastTechFoodsAuth.Security.Middleware;

namespace FastTechFoodsAuth.Security.Extensions;

/// <summary>
/// Extensões para configuração de middleware de segurança
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adiciona o middleware de auditoria de segurança
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder para encadeamento</returns>
    public static IApplicationBuilder UseFastTechFoodsSecurityAudit(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityAuditMiddleware>();
    }
}
