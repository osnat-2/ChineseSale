using AutoMapper;
using Project.Dto;
using Project.Models;

namespace Project
{
    public class CardProfile: Profile
    {
        public CardProfile()
        {
            CreateMap<CardDto, Card>();
            // .ForMember(destination => destination.IsPaid, options => options.MapFrom(_ => false))
            // .ForMember(destination => destination.CreatedAt, options => options.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
