using AutoMapper;
using FastTechFoodsAuth.Application.DTOs;
using FastTechFoodsAuth.Domain.Entities;

namespace FastTechFoodsAuth.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));

            CreateMap<RegisterUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Hash será atribuído manualmente
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore());    // Atribuição manual
        }
    }
}
