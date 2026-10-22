using AutoMapper;
using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Bll
{
    public class LotteryService : ILotteryService
    {
        private readonly ILotteryDal _lotteryDal;
        private readonly IMapper _mapper;

        public LotteryService(ILotteryDal lotteryDal, IMapper mapper)
        {
            _lotteryDal = lotteryDal;
            _mapper = mapper;
        }

        public Task<Result<Lottery>> GetLotteriesAsync() =>
            _lotteryDal.GetLotteriesAsync();

        public Task<Result<Lottery>> AddLotteryAsync(LotteryDto lotteryDto)
        {
            if (lotteryDto == null || lotteryDto.Time == default)
            {
                return Task.FromResult(Failure("A valid lottery date is required."));
            }

            return _lotteryDal.AddLotteryAsync(_mapper.Map<Lottery>(lotteryDto));
        }

        public Task<Result<Lottery>> UpdateLotteryAsync(int id, LotteryDto lotteryDto)
        {
            if (id <= 0 || lotteryDto == null || lotteryDto.Time == default)
            {
                return Task.FromResult(Failure("A valid lottery id and date are required."));
            }

            return _lotteryDal.UpdateLotteryAsync(id, lotteryDto.Time);
        }

        private static Result<Lottery> Failure(string message) => new()
        {
            Success = false,
            Message = message,
            Data = null
        };
    }
}