using AutoMapper;
using Project.BLL.Interfaces;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.BLL
{
    public class LotteryService : ILotteryService
    {
        private readonly IMapper _mapper;
        public LotteryService(IMapper mapper)
        {
            _mapper = mapper;
        }
    }
}