using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FastTechFoodsAuth.Security.Middleware;

public class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 401)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var path = context.Request.Path;
            var method = context.Request.Method;
            var hasAuthHeader = context.Request.Headers.ContainsKey("Authorization");

            _logger.LogWarning("Tentativa de acesso não autorizado: {Method} {Path} | IP: {IP} | UserAgent: {UserAgent} | HasAuthHeader: {HasAuthHeader}",
                method, path, ipAddress, userAgent, hasAuthHeader);
        }
        
        if (context.Response.StatusCode == 403)
        {
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var path = context.Request.Path;
            var method = context.Request.Method;
            var user = context.User?.Identity?.Name ?? "Unknown";

            _logger.LogWarning("Acesso negado para usuário autenticado: {Method} {Path} | User: {User} | IP: {IP} | UserAgent: {UserAgent}",
                method, path, user, ipAddress, userAgent);
        }
    }
}
