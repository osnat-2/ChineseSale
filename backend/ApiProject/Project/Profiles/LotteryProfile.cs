using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles
{
    public class LotteryProfile: Profile
    {
        public LotteryProfile()
        {
            CreateMap<LotteryDto, Lottery>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.IsDeleted, options => options.Ignore())
                .ForMember(destination => destination.DeletedAt, options => options.Ignore())
                .ForMember(destination => destination.CreatedAt, options => options.Ignore())
                .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedByUser, options => options.Ignore());
            // .ForMember(destination => destination.IsMadeOut, options => options.MapFrom(_ => false));
        }
    }
}