using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles;

public class WinnerProfile : Profile
{
    public WinnerProfile()
    {
        CreateMap<WinnerDto, Winner>()
            .ForMember(destination => destination.Id, options => options.Ignore())
            .ForMember(destination => destination.IsDeleted, options => options.Ignore())
            .ForMember(destination => destination.DeletedAt, options => options.Ignore())
            .ForMember(destination => destination.CreatedAt, options => options.Ignore())
            .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
            .ForMember(destination => destination.CreatedBy, options => options.Ignore())
            .ForMember(destination => destination.CreatedByUser, options => options.Ignore())
            .ForMember(destination => destination.Present, options => options.Ignore())
            .ForMember(destination => destination.Card, options => options.Ignore())
            .ForMember(destination => destination.Lottery, options => options.Ignore());
    }
}