using System.Net;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;

namespace FastTechFoodsAuth.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro não tratado: {Message} | Path: {Path} | Method: {Method}", 
                    ex.Message, context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            object response;

            switch (exception)
            {
                case ValidationException validationEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new 
                    { 
                        message = "Erro de validação",
                        errors = validationEx.Errors.Select(e => new { 
                            field = e.PropertyName, 
                            error = e.ErrorMessage 
                        }),
                        timestamp = DateTime.UtcNow
                    };
                    break;

                case SecurityTokenExpiredException expiredEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Token expirado", 
                        details = $"Token expirou em {expiredEx.Expires:yyyy-MM-dd HH:mm:ss UTC}",
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case SecurityTokenInvalidSignatureException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Token com assinatura inválida",
                        details = "A assinatura do token não é válida",
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case SecurityTokenInvalidIssuerException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Token com issuer inválido",
                        details = "O issuer do token não é válido",
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case SecurityTokenInvalidAudienceException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Token com audience inválido",
                        details = "O audience do token não é válido",
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case SecurityTokenException tokenEx:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Erro de token JWT",
                        details = tokenEx.Message,
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new { 
                        message = "Acesso não autorizado",
                        details = "Você não tem permissão para acessar este recurso",
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case ArgumentException argEx when argEx.Message.Contains("email"):
                case ArgumentException argEx2 when argEx2.Message.Contains("Email"):
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new { 
                        message = "Email inválido",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow 
                    };
                    break;

                case InvalidOperationException opEx when opEx.Message.Contains("already"):
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response = new { 
                        message = "Conflito de dados",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow 
                    };
                    break;
                
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new
                    {
                        message = "Ocorreu um erro interno no servidor",
                        details = exception.Message,
                        timestamp = DateTime.UtcNow
                    };
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
