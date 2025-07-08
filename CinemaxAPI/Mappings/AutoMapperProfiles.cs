using AutoMapper;
using CinemaxAPI.Models.Domain;
using CinemaxAPI.Models.DTO;
using CinemaxAPI.Models.DTO.Requests;

namespace CinemaxAPI.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Add your mapping configurations here
            // CreateMap<Source, Destination>();
            CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTime.UtcNow))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty));
            CreateMap<ApplicationUser, ManagerDTO>()
                .ForMember(dest => dest.Theater, opt => opt.MapFrom(src => src.ManagedTheater))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTime.UtcNow));
            CreateMap<ApplicationUser, EmployeeDTO>()
                .ForMember(dest => dest.Theater, opt => opt.MapFrom(src => src.EmployedTheater))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.LockoutEnd.HasValue && src.LockoutEnd.Value > DateTime.UtcNow));

            CreateMap<Province, ProvinceDTO>();

            CreateMap<Theater, TheaterDTO>()
                .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province != null ? src.Province : null));

            // Movie mappings
            CreateMap<Movie, MovieDTO>();
            CreateMap<CreateMovieRequestDTO, Movie>();
            CreateMap<UpdateMovieRequestDTO, Movie>();

            CreateMap<Screen, ScreenDTO>()
                .ForMember(dest => dest.TheaterName, opt => opt.MapFrom(src => src.Theater != null ? src.Theater.Name : string.Empty));

            // ShowTime mappings
            CreateMap<ShowTime, ShowTimeDTO>();
            CreateMap<CreateShowTimeRequestDTO, ShowTime>();
            CreateMap<UpdateShowTimeRequestDTO, ShowTime>();
            CreateMap<Seat, SeatDTO>();
        }
    }

}
