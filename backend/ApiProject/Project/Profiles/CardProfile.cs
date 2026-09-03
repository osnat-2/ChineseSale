using AutoMapper;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project
{
    public class CardProfile: Profile
    {
        public CardProfile()
        {
            CreateMap<CardDto, Card>()
            .ForMember(destination => destination.IsPaid, options => options.MapFrom(_ => false))
            .ForMember(destination => destination.CreatedAt, options => options.MapFrom(_ => DateTime.UtcNow));
        }
        static int Id = 0;
        private int Identity()
        {
            Id++;
            return Id;
        }
    }
}
