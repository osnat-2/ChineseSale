using AutoMapper;
using Project.Dto;
using Project.Models;

namespace Project
{
    public class CardProfile: Profile
    {
        public CardProfile()
        {
            CreateMap<CardDto, Card>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.IsPaid, options => options.Ignore())
                .ForMember(destination => destination.IsDeleted, options => options.Ignore())
                .ForMember(destination => destination.DeletedAt, options => options.Ignore())
                .ForMember(destination => destination.CreatedAt, options => options.Ignore())
                .ForMember(destination => destination.UpdatedAt, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedByUser, options => options.Ignore())
                .ForMember(destination => destination.Present, options => options.Ignore());
            // .ForMember(destination => destination.IsPaid, options => options.MapFrom(_ => false))
            // .ForMember(destination => destination.CreatedAt, options => options.MapFrom(_ => DateTime.UtcNow));
        }
    }
}
