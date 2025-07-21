using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace FastTechFoodsAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra um novo usuário.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto dto)
        {
            try
            {
                var user = await _userService.RegisterAsync(dto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Realiza login por Email ou CPF.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginRequestDto dto, IValidator<LoginRequestDto> validator)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
            }
            try
            {
                var result = await _userService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Busca usuário autenticado (profile).
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me()
        {
            try
            {
                // Verifica se o usuário está autenticado
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return Unauthorized(new { 
                        message = "Token não fornecido ou inválido",
                        details = "O header Authorization com Bearer token é obrigatório",
                        timestamp = DateTime.UtcNow 
                    });
                }

                // Busca o claim 'sub' (subject) que contém o ID do usuário
                // Tenta diferentes formas de localizar a claim 'sub'
                var userIdClaim = User.FindFirst("sub")?.Value 
                    ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    // Debug: Mostra todas as claims disponíveis para diagnóstico
                    var availableClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
                    
                    return Unauthorized(new { 
                        message = "Token inválido", 
                        details = "Claim 'sub' não encontrado no token",
                        availableClaims = availableClaims, // Para debug
                        timestamp = DateTime.UtcNow 
                    });
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new { 
                        message = "Token inválido", 
                        details = "Claim 'sub' não contém um GUID válido",
                        timestamp = DateTime.UtcNow 
                    });
                }

                var user = await _userService.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { 
                        message = "Usuário não encontrado", 
                        details = $"Nenhum usuário encontrado com ID: {userId}",
                        timestamp = DateTime.UtcNow 
                    });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erro interno do servidor", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Endpoint para teste de autorização por role (apenas Admin).
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public ActionResult AdminOnly()
        {
            var userIdClaim = User.FindFirst("sub")?.Value;
            var userName = User.FindFirst("name")?.Value;
            var userRoles = User.FindFirst("roles")?.Value;

            return Ok(new { 
                message = "Acesso autorizado para Admin", 
                userId = userIdClaim, 
                userName = userName,
                roles = userRoles 
            });
        }

        /// <summary>
        /// Endpoint para diagnóstico de token JWT (desenvolvimento).
        /// </summary>
        [HttpGet("token-info")]
        [Authorize]
        public ActionResult TokenInfo()
        {
            try
            {
                var claims = User.Claims.Select(c => new { 
                    Type = c.Type, 
                    Value = c.Value,
                    // Adiciona informação se é uma claim padrão
                    IsStandardClaim = c.Type.StartsWith("http://schemas") || 
                                     c.Type == "sub" || c.Type == "email" || c.Type == "name" || c.Type == "roles"
                }).ToList();
                
                var identity = User.Identity;

                // Tenta diferentes formas de obter o ID do usuário
                var userIdFromSub = User.FindFirst("sub")?.Value;
                var userIdFromJwtSub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var userIdFromNameIdentifier = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

                return Ok(new
                {
                    isAuthenticated = identity?.IsAuthenticated ?? false,
                    authenticationType = identity?.AuthenticationType,
                    name = identity?.Name,
                    userIdDebugging = new
                    {
                        fromSub = userIdFromSub,
                        fromJwtSub = userIdFromJwtSub,
                        fromNameIdentifier = userIdFromNameIdentifier
                    },
                    totalClaims = claims.Count,
                    claims = claims,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Erro ao analisar token", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }
    }
}
