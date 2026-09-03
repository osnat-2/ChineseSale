using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles
{
    public class DonorProfile: Profile
    {
        public DonorProfile()
        {
        CreateMap<DonorDto, Donor>()
            .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true))
            .ForMember(destination => destination.CreatedAt, options => options.MapFrom(_ => DateTime.UtcNow));
        }
    }
}