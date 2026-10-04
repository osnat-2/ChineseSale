using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface IWinnerDal
    {
        Task<Result<Winner>> DrawWinnerForPresentAsync(int presentId, int? lotteryId = null);
        //Task<Result<Dictionary<Present, List<User>>>> GetPresentsWithUsersAsync();
        //Task<decimal> CalculateTotalIncomeForPresentAsync();
    }
}
