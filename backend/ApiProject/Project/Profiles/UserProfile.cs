using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, User>();
            // .ForMember(destination => destination.RegisterationTime, options => options.MapFrom(_ => DateTime.UtcNow))
            // .ForMember(destination => destination.Role, options => options.MapFrom(_ => "User"))
            // .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true));
        }
    }
}