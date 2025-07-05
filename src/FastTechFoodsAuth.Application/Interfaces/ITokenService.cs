using FastTechFoodsAuth.Domain.Entities;

namespace FastTechFoodsAuth.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken(User user);
    }
}
