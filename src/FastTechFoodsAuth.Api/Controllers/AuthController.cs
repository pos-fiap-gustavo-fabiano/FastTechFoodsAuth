using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using FastTechFoodsOrder.Shared.Controllers;
using FastTechFoodsOrder.Shared.Results;

namespace FastTechFoodsAuth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
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
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return ToActionResult(result);
        }

        /// <summary>
        /// Realiza login por Email ou CPF.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, IValidator<LoginRequestDto> validator)
        {
            var validation = await validator.ValidateAsync(dto);
            if (!validation.IsValid)
            {
                var validationErrors = validation.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { 
                    message = "Dados de entrada inválidos", 
                    errors = validationErrors,
                    timestamp = DateTime.UtcNow 
                });
            }

            var result = await _userService.LoginAsync(dto);
            return ToActionResult(result);
        }

        /// <summary>
        /// Busca usuário autenticado (profile).
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userIdResult = GetUserIdFromToken();
            if (!userIdResult.IsSuccess)
                return ToActionResult(userIdResult);

            var result = await _userService.GetByIdAsync(userIdResult.Value);
            return ToActionResult(result);
        }

        /// <summary>
        /// Endpoint para teste de autorização por role (apenas Admin).
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            var userIdResult = GetUserIdFromToken();
            if (!userIdResult.IsSuccess)
                return ToActionResult(userIdResult);

            var userName = User.FindFirst("name")?.Value;
            var userRoles = User.FindFirst("roles")?.Value;

            return Ok(new { 
                message = "Acesso autorizado para Admin", 
                userId = userIdResult.Value, 
                userName = userName,
                roles = userRoles 
            });
        }

        [HttpGet("token-info")]
        [Authorize]
        public IActionResult TokenInfo()
        {
            var claims = User.Claims.Select(c => new { 
                Type = c.Type, 
                Value = c.Value,
                IsStandardClaim = c.Type.StartsWith("http://schemas") || 
                                 c.Type == "sub" || c.Type == "email" || c.Type == "name" || c.Type == "roles"
            }).ToList();
            
            var identity = User.Identity;
            var userIdResult = GetUserIdFromToken();

            return Ok(new
            {
                isAuthenticated = identity?.IsAuthenticated ?? false,
                authenticationType = identity?.AuthenticationType,
                name = identity?.Name,
                userId = userIdResult.IsSuccess ? userIdResult.Value : (Guid?)null,
                userIdError = !userIdResult.IsSuccess ? userIdResult.ErrorMessage : null,
                totalClaims = claims.Count,
                claims = claims,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Extrai o ID do usuário do token JWT.
        /// </summary>
        private Result<Guid> GetUserIdFromToken()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Result<Guid>.Failure("Token não fornecido ou inválido", "UNAUTHORIZED");
            }

            var userIdClaim = User.FindFirst("sub")?.Value 
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Result<Guid>.Failure("Token inválido - Claim 'sub' não encontrado", "UNAUTHORIZED");
            }

            if (!Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Result<Guid>.Failure("Token inválido - Claim 'sub' não contém um GUID válido", "UNAUTHORIZED");
            }

            return Result<Guid>.Success(userId);
        }
    }
}
