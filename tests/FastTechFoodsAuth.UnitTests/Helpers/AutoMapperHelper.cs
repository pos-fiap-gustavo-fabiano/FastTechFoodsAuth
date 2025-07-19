using AutoMapper;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Domain.Entities;
using Moq;

namespace FastTechFoodsAuth.UnitTests.Helpers
{
    public static class AutoMapperHelper
    {
        public static IMapper CreateMapper()
        {
            // For now, create a simple mapper configuration manually
            // until we resolve the AutoMapper version issue
            var mockMapper = new Mock<IMapper>();
            
            // Setup RegisterUserDto -> User mapping
            mockMapper.Setup(m => m.Map<User>(It.IsAny<RegisterUserDto>()))
                .Returns((RegisterUserDto dto) => new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    CPF = dto.CPF,
                    UserRoles = new List<UserRole>()
                });
            
            // Setup User -> UserDto mapping
            mockMapper.Setup(m => m.Map<UserDto>(It.IsAny<User>()))
                .Returns((User user) => new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CPF = user.CPF,
                    Roles = user.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(r => !string.IsNullOrEmpty(r)).ToList() ?? new List<string>()
                });
                
            return mockMapper.Object;
        }
    }
}
