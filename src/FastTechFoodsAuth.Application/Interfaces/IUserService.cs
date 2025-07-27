using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsOrder.Shared.Results;

namespace FastTechFoodsAuth.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserDto>> RegisterAsync(RegisterUserDto input);
        Task<Result<AuthResultDto>> LoginAsync(LoginRequestDto input);
        Task<Result<UserDto>> GetByIdAsync(Guid id);
    }
}
