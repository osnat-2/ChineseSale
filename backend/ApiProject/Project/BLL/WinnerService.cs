using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Models;

namespace Project.Bll
{
    public class WinnerService : IWinnerService
    {
        private readonly IWinnerDal _winnerDal;
        private readonly IEmailService _emailService;
        
        public WinnerService(IWinnerDal winnerDal, IEmailService emailService)
        {
            _winnerDal = winnerDal;
            _emailService = emailService;
        }

        //public async Task<Result<Winner>> DrawWinnerForPresentAsync(int presentId)
        //{
        //    if (presentId <= 0)
        //    {
        //        return new Result<Winner>
        //        {
        //            Success = false,
        //            Message = "Invalid present id",
        //            Data = null
        //        };
        //    }

        //    return await _winnerDAL.DrawWinnerForPresentAsync(presentId);
        //}

        //public async Task<Result<Dictionary<Present, List<User>>>> GetPresentsWithUsersAsync()
        //{
        //    return await _winnerDAL.GetPresentsWithUsersAsync();
        //}
        //public async Task<decimal> CalculateTotalIncomeForPresentAsync()
        //{
        //    return await _winnerDAL.CalculateTotalIncomeForPresentAsync();
        //}
        //public async Task<Result<string>> SendWinnerEmailAsync()
        //{
        //    return await _emailService.SendAllWinnersEmailAsync();
        //}
    }
}