using FastTechFoodsAuth.Application.DTOs;

namespace FastTechFoodsAuth.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(RegisterUserDto input);
        Task<AuthResultDto> LoginAsync(LoginRequestDto input);
        Task<UserDto> GetByIdAsync(Guid id);
    }
}
