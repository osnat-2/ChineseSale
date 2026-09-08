using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Dal.Interfaces;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Dal
{
    public class LotteryDal : ILotteryDal
    {
        private readonly AppDBContext dbContext;
        private readonly ILogger<LotteryDal> logger;

        public LotteryDal(AppDBContext dbContext, ILogger<LotteryDal> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }
        // Helper method to check if raffle has occurred
        private async Task<bool> HasRaffleOccurredAsync()
        {
            return await dbContext.Lottery.AnyAsync();
            // return await dbContext.Lottery.FirstOrDefaultAsync(l => l.Time < DateTime.Now.Date) != null;
        }
    }
}