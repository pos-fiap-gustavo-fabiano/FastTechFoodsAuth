using AutoMapper;
using BCrypt.Net;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Application.Interfaces;
using FastTechFoodsAuth.Domain.Entities;

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

        public async Task<UserDto> RegisterAsync(RegisterUserDto input)
        {
            var existing = await _userRepository.GetByEmailAsync(input.Email);
            if (existing != null)
                throw new Exception("Email already in use.");

            var roleName = string.IsNullOrEmpty(input.Role) ? "Client" : input.Role;
            var role = await _roleRepository.GetByNameAsync(roleName);
            if (role == null)
                throw new Exception("Role not found.");

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

            return _mapper.Map<UserDto>(user);
        }

        public async Task<AuthResultDto> LoginAsync(LoginRequestDto input)
        {
            User user = null;
            if (input.EmailOrCpf.Contains("@"))
                user = await _userRepository.GetByEmailAsync(input.EmailOrCpf);
            else
                user = await _userRepository.GetByCpfAsync(input.EmailOrCpf);

            if (user == null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
                throw new Exception("Invalid credentials.");

            // Gere o token JWT e refresh token
            var token = _tokenService.GenerateJwtToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user);

            return new AuthResultDto
            {
                Token = token,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }
    }
}
