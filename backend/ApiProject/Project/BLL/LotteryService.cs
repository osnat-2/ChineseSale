using AutoMapper;
using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Models;

namespace Project.Bll
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