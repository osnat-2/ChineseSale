using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project
{
    public class PresentProfile: Profile
    {
        public PresentProfile()
        {
            CreateMap<PresentDto, Present>()
            .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true));
        }
    }
}