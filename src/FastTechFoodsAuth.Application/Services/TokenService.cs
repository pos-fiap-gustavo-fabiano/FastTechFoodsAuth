using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FastTechFoodsAuth.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name),
                new Claim("roles", string.Join(",", user.UserRoles?.Select(ur => ur.Role.Name) ?? new List<string>()))
            };
            var JWT_SECRET = Environment.GetEnvironmentVariable("JWT_SECRET");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_SECRET));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(User user)
        {
            // Simplificação para MVP, gere um token seguro em produção!
            return Guid.NewGuid().ToString("N");
        }
    }
}
