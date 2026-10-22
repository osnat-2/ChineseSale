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

        public async Task<Result<Winner>> DrawWinnerForPresentAsync(int presentId, int? lotteryId = null)
        {
            if (presentId <= 0 || !lotteryId.HasValue || lotteryId.Value <= 0)
            {
                return new Result<Winner>
                {
                    Success = false,
                    Message = "A valid present id and annual lottery id are required.",
                    Data = null
                };
            }

            return await _winnerDal.DrawWinnerForPresentAsync(presentId, lotteryId);
        }

        public Task<Result<Winner>> ProcessLotteryAsync(int lotteryId) =>
            lotteryId > 0
                ? _winnerDal.ProcessLotteryAsync(lotteryId)
                : Task.FromResult(new Result<Winner>
                {
                    Success = false,
                    Message = "Invalid lottery id.",
                    Data = null
                });

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