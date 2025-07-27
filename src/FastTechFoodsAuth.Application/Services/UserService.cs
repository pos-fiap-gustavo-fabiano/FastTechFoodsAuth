using AutoMapper;
using BCrypt.Net;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Domain.Entities;
using FastTechFoodsOrder.Shared.Results;

namespace FastTechFoodsAuth.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<Result<UserDto>> RegisterAsync(RegisterUserDto input)
        {
            try
            {
                var existing = await _userRepository.GetByEmailAsync(input.Email);
                if (existing != null)
                    return Result<UserDto>.Failure("Email already in use.", "VALIDATION_ERROR");

                var roleName = string.IsNullOrEmpty(input.Role) ? "Client" : input.Role;
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null)
                    return Result<UserDto>.Failure("Role not found.", "VALIDATION_ERROR");

                var user = _mapper.Map<User>(input);
                user.Id = Guid.NewGuid();
                user.CreatedAt = DateTime.UtcNow;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password);
                user.UserRoles = new List<UserRole>
                {
                    new UserRole { UserId = user.Id, RoleId = role.Id }
                };

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Failure($"Error registering user: {ex.Message}", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<AuthResultDto>> LoginAsync(LoginRequestDto input)
        {
            try
            {
                User? user = null;
                if (input.EmailOrCpf.Contains("@"))
                    user = await _userRepository.GetByEmailAsync(input.EmailOrCpf);
                else
                    user = await _userRepository.GetByCpfAsync(input.EmailOrCpf);

                if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                    return Result<AuthResultDto>.Failure("Invalid credentials.", "UNAUTHORIZED");

                // Gere o token JWT e refresh token
                var token = _tokenService.GenerateJwtToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken(user);

                var authResult = new AuthResultDto
                {
                    Access_Token = token,
                    RefreshToken = refreshToken,
                    User = _mapper.Map<UserDto>(user)
                };

                return Result<AuthResultDto>.Success(authResult);
            }
            catch (Exception ex)
            {
                return Result<AuthResultDto>.Failure($"Error during login: {ex.Message}", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<UserDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return Result<UserDto>.Failure("User not found.", "NOT_FOUND");

                var userDto = _mapper.Map<UserDto>(user);
                return Result<UserDto>.Success(userDto);
            }
            catch (Exception ex)
            {
                return Result<UserDto>.Failure($"Error retrieving user: {ex.Message}", "INTERNAL_ERROR");
            }
        }
    }
}
