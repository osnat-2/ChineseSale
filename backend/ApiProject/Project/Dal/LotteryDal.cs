using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Project.Dal.Interfaces;
using Project.Models;
using System.Data;

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

        public async Task<Result<Lottery>> GetLotteriesAsync()
        {
            try
            {
                var lotteries = await dbContext.Lottery
                    .AsNoTracking()
                    .OrderByDescending(lottery => lottery.Time)
                    .ToListAsync();
                return new Result<Lottery>
                {
                    Success = true,
                    Message = "Lotteries fetched successfully.",
                    Data = lotteries
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to fetch lotteries.");
                return Failure("Unable to fetch lotteries.");
            }
        }

        public async Task<Result<Lottery>> AddLotteryAsync(Lottery lottery)
        {
            try
            {
                await using var transaction = await BeginSerializableTransactionAsync();
                var existingForYear = await dbContext.Lottery
                    .AnyAsync(existing => existing.Time.Year == lottery.Time.Year);
                if (existingForYear)
                {
                    return Failure($"A lottery already exists for {lottery.Time.Year}.");
                }

                var now = DateTime.UtcNow;
                lottery.IsMadeOut = false;
                lottery.IsActive = true;
                lottery.IsDeleted = false;
                lottery.CreatedAt = now;
                lottery.CreatedBy = 0;
                lottery.UpdatedAt = null;
                lottery.DeletedAt = null;
                await dbContext.Lottery.AddAsync(lottery);
                await dbContext.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return new Result<Lottery>
                {
                    Success = true,
                    Message = "Lottery created successfully.",
                    Data = new[] { lottery }
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to create annual lottery for {Year}.", lottery.Time.Year);
                return Failure("Unable to create the lottery. Please try again.");
            }
        }

        public async Task<Result<Lottery>> UpdateLotteryAsync(int id, DateOnly time)
        {
            try
            {
                await using var transaction = await BeginSerializableTransactionAsync();
                var lottery = await dbContext.Lottery
                    .FirstOrDefaultAsync(existing => existing.Id == id && existing.IsActive);
                if (lottery == null)
                {
                    return Failure("Lottery not found.");
                }

                if (lottery.IsMadeOut || await dbContext.Winner.AnyAsync(winner =>
                        winner.LotteryId == id && winner.IsActive && !winner.IsDeleted))
                {
                    return Failure("A lottery cannot be changed after processing has started.");
                }

                var anotherLotteryForYear = await dbContext.Lottery
                    .AnyAsync(existing => existing.Id != id && existing.Time.Year == time.Year);
                if (anotherLotteryForYear)
                {
                    return Failure($"A lottery already exists for {time.Year}.");
                }

                lottery.Time = time;
                lottery.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                return new Result<Lottery>
                {
                    Success = true,
                    Message = "Lottery updated successfully.",
                    Data = new[] { lottery }
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to update lottery {LotteryId}.", id);
                return Failure("Unable to update the lottery. Please try again.");
            }
        }

        private Task<IDbContextTransaction?> BeginSerializableTransactionAsync() =>
            dbContext.Database.IsRelational()
                ? BeginTransactionAsync()
                : Task.FromResult<IDbContextTransaction?>(null);

        private async Task<IDbContextTransaction?> BeginTransactionAsync() =>
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        private static Result<Lottery> Failure(string message) => new()
        {
            Success = false,
            Message = message,
            Data = null
        };
    }
}
