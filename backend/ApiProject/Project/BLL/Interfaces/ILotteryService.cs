using Project.Models;
using Project.Dto;

namespace Project.Bll.Interfaces
{
    public interface ILotteryService
    {
        Task<Result<Lottery>> GetLotteriesAsync();
        Task<Result<Lottery>> AddLotteryAsync(LotteryDto lotteryDto);
        Task<Result<Lottery>> UpdateLotteryAsync(int id, LotteryDto lotteryDto);
    }
}
