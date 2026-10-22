using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface ILotteryDal
    {
        Task<Result<Lottery>> GetLotteriesAsync();
        Task<Result<Lottery>> AddLotteryAsync(Lottery lottery);
        Task<Result<Lottery>> UpdateLotteryAsync(int id, DateOnly time);
    }
}
