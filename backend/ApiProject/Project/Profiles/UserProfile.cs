using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.RoleId, options => options.Ignore())
                .ForMember(destination => destination.Role, options => options.Ignore())
                .ForMember(destination => destination.IsActive, options => options.Ignore())
                .ForMember(destination => destination.IsDeleted, options => options.Ignore())
                .ForMember(destination => destination.CreatedAt, options => options.Ignore())
                .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
                .ForMember(destination => destination.DeletedAt, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedByUser, options => options.Ignore());
            // .ForMember(destination => destination.RegisterationTime, options => options.MapFrom(_ => DateTime.UtcNow))
            // .ForMember(destination => destination.Role, options => options.MapFrom(_ => "User"))
            // .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true));
        }
    }
}