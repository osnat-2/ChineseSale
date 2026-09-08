using AutoMapper;
using Project.Models;
using Project.Dto;

namespace Project.Profiles;

public class WinnerProfile : Profile
{
    public WinnerProfile()
    {
        CreateMap<WinnerDto, Winner>();
    }
}