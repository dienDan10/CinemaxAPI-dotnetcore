using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;

namespace CinemaxAPI.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Add your mapping configurations here
            // CreateMap<Source, Destination>();
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTime.UtcNow))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty));
            CreateMap<ApplicationUser, ManagerDTO>()
                .ForMember(dest => dest.Theater, opt => opt.MapFrom(src => src.ManagedTheater))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTime.UtcNow));

            CreateMap<Province, ProvinceDTO>();

            CreateMap<Theater, TheaterDTO>()
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province != null ? src.Province : null));
        }
    }

}
