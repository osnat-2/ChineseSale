using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Dal.Interfaces;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Dal
{
    public class WinnerDal : IWinnerDal
    {
        private readonly AppDBContext dbContext;
        private readonly ILogger<WinnerDal> logger;

        public WinnerDal(AppDBContext dbContext, ILogger<WinnerDal> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Result<Winner>> DrawWinnerForPresentAsync(int presentId, int? lotteryId = null)
        {
            try
            {
                if (presentId <= 0)
                {
                    logger.LogWarning("Draw attempted with invalid present id {PresentId}.", presentId);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "Invalid present id.",
                        Data = null
                    };
                }

                var present = await dbContext.Present
                    .FirstOrDefaultAsync(p => p.Id == presentId && p.IsActive && !p.IsDeleted);

                if (present == null)
                {
                    logger.LogWarning("No active present found for draw. PresentId={PresentId}", presentId);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = $"Present with id {presentId} was not found or is not active.",
                        Data = null
                    };
                }

                var eligibleCards = await dbContext.Card
                    .Where(c => c.PresentId == presentId && c.IsPaid && c.IsActive && !c.IsDeleted)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                if (!eligibleCards.Any())
                {
                    logger.LogWarning("Draw rejected for present {PresentId}: no paid cards are available.", presentId);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "No paid cards are available for this present, so a draw cannot be completed.",
                        Data = null
                    };
                }

                var lottery = await ResolveLotteryAsync(lotteryId);
                if (lottery == null)
                {
                    logger.LogWarning("Lottery lookup failed for present {PresentId} and lottery id {LotteryId}.", presentId, lotteryId);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = lotteryId.HasValue && lotteryId.Value > 0
                            ? $"Lottery with id {lotteryId.Value} was not found or is already deleted."
                            : "Unable to create or resolve the draw record.",
                        Data = null
                    };
                }

                if (lottery.IsMadeOut)
                {
                    logger.LogWarning("Draw rejected because lottery {LotteryId} is already completed.", lottery.Id);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "This lottery has already been completed.",
                        Data = null
                    };
                }

                var selectedCard = eligibleCards[Random.Shared.Next(eligibleCards.Count)];
                var winner = new Winner
                {
                    PresentId = presentId,
                    CardId = selectedCard.Id,
                    LotteryId = lottery.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 0
                };

                dbContext.Winner.Add(winner);

                lottery.IsMadeOut = true;
                lottery.UpdatedAt = DateTime.UtcNow;
                dbContext.Lottery.Update(lottery);

                if (dbContext.Database.IsRelational())
                {
                    using var transaction = await dbContext.Database.BeginTransactionAsync();
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await dbContext.SaveChangesAsync();
                }

                logger.LogInformation("Winner drawn successfully for present {PresentId}, lottery {LotteryId}, card {CardId}.", presentId, lottery.Id, selectedCard.Id);

                return new Result<Winner>
                {
                    Success = true,
                    Message = "Winner selected successfully.",
                    Data = new[] { winner }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while drawing winner for present with id {PresentId}.", presentId);
                return new Result<Winner>
                {
                    Success = false,
                    Message = "An error occurred while drawing the winner. Please try again later.",
                    Data = null
                };
            }
        }

        private async Task<Lottery?> ResolveLotteryAsync(int? lotteryId)
        {
            if (lotteryId.HasValue && lotteryId.Value > 0)
            {
                return await dbContext.Lottery
                    .FirstOrDefaultAsync(l => l.Id == lotteryId.Value && !l.IsDeleted);
            }

            var lottery = new Lottery
            {
                Time = DateOnly.FromDateTime(DateTime.UtcNow),
                IsMadeOut = false,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 0
            };

            await dbContext.Lottery.AddAsync(lottery);
            await dbContext.SaveChangesAsync();
            return lottery;
        }

        //??? for all the present not to a specific!!
        public async Task<Result<Dictionary<Present, List<User>>>> GetPresentsWithUsersAsync()
        {
            try
            {
                var winners = await dbContext.Winner
                    .ToListAsync();

                var presentsWithUsers = winners
                    .GroupBy(w => w.PresentId);

                return new Result<Dictionary<Present, List<User>>>
                {
                    Success = true,
                    Message = "Presents with users fetched successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching presents with users.");

                return new Result<Dictionary<Present, List<User>>>
                {
                    Success = false,
                    Message = "An error occurred while fetching presents with users.",
                    Data = null
                };
            }
        }

        public async Task<decimal> CalculateTotalIncomeForPresentAsync()
        {
            try
            {
                var totalIncome = await dbContext.Card
                    .Where(c => c.IsPaid == true)
                    .SumAsync(c => c.Present.Price);

                logger.LogInformation("Total income for all presents is {TotalIncome}.", totalIncome);
                return totalIncome;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while calculating total income for all presents.");
                return 0m;
            }
        }
    }
}