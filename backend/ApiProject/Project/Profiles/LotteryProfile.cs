using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles
{
    public class LotteryProfile: Profile
    {
        public LotteryProfile()
        {
        CreateMap<LotteryDto, Lottery>();
            // .ForMember(destination => destination.IsMadeOut, options => options.MapFrom(_ => false));
        }
    }
}