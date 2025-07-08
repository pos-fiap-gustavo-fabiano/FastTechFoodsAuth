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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", user.Name),
                new Claim("roles", string.Join(",", user.UserRoles?.Select(ur => ur.Role.Name) ?? new List<string>()))
            };

            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
                ?? _configuration["Jwt:Key"];
            
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "2");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHours),
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
