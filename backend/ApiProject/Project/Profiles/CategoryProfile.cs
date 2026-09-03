using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project
{
    public class CategoryProfile: Profile
    {
        public CategoryProfile()
        {
        CreateMap<CategoryDto, Category>();
            // .ForMember(destination => destination.IsActive, options => options.MapFrom(_ => true));
        }
    }
}