using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Dal.Interfaces;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Data;
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

                await using var transaction = dbContext.Database.IsRelational()
                    ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                    : null;

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

                if (present.Quantity <= 0)
                {
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "The present must have at least one configured winner.",
                        Data = null
                    };
                }

                if (!lotteryId.HasValue || lotteryId.Value <= 0)
                {
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "A valid annual lottery id is required.",
                        Data = null
                    };
                }

                var lottery = await dbContext.Lottery
                    .FirstOrDefaultAsync(existing =>
                        existing.Id == lotteryId.Value && existing.IsActive && !existing.IsDeleted);
                if (lottery == null)
                {
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "Lottery not found.",
                        Data = null
                    };
                }

                var drawAlreadyExists = await dbContext.Winner
                    .AnyAsync(w => w.PresentId == presentId &&
                                   w.LotteryId == lottery.Id &&
                                   w.IsActive &&
                                   !w.IsDeleted);
                if (drawAlreadyExists)
                {
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = "This present has already been drawn.",
                        Data = null
                    };
                }

                var eligibleCards = await dbContext.Card
                    .Where(c => c.PresentId == presentId && c.IsPaid && c.IsActive && !c.IsDeleted)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                if (eligibleCards.Count < present.Quantity)
                {
                    logger.LogWarning(
                        "Draw rejected for present {PresentId}: {EligibleCount} paid cards are available for {RequiredWinnerCount} winners.",
                        presentId,
                        eligibleCards.Count,
                        present.Quantity);
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = $"Not enough paid cards are available. This present requires {present.Quantity} distinct paid cards.",
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

                for (var index = 0; index < present.Quantity; index++)
                {
                    var selectedIndex = Random.Shared.Next(index, eligibleCards.Count);
                    (eligibleCards[index], eligibleCards[selectedIndex]) =
                        (eligibleCards[selectedIndex], eligibleCards[index]);
                }

                var now = DateTime.UtcNow;
                var winners = eligibleCards.Take(present.Quantity).Select(card => new Winner
                {
                    PresentId = presentId,
                    CardId = card.Id,
                    Lottery = lottery,
                    LotteryId = lottery.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = now,
                    CreatedBy = 0
                }).ToList();

                dbContext.Winner.AddRange(winners);
                await dbContext.SaveChangesAsync();
                lottery.IsMadeOut = await AreAllActivePresentsDrawnAsync(lottery.Id);
                lottery.UpdatedAt = now;
                dbContext.Lottery.Update(lottery);
                await dbContext.SaveChangesAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                logger.LogInformation(
                    "Winner draw completed for present {PresentId}, lottery {LotteryId}, winner count {WinnerCount}.",
                    presentId,
                    lottery.Id,
                    winners.Count);

                return new Result<Winner>
                {
                    Success = true,
                    Message = "Winners selected successfully.",
                    Data = winners
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

        public async Task<Result<Winner>> ProcessLotteryAsync(int lotteryId)
        {
            try
            {
                if (lotteryId <= 0)
                {
                    return Failure("Invalid lottery id.");
                }

                await using var transaction = dbContext.Database.IsRelational()
                    ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                    : null;

                var lottery = await dbContext.Lottery
                    .FirstOrDefaultAsync(existing =>
                        existing.Id == lotteryId && existing.IsActive && !existing.IsDeleted);
                if (lottery == null)
                {
                    return Failure("Lottery not found.");
                }
                if (lottery.IsMadeOut)
                {
                    return Failure("This lottery has already been completed.");
                }

                var presents = await dbContext.Present
                    .Where(present => present.IsActive && !present.IsDeleted)
                    .OrderBy(present => present.Id)
                    .ToListAsync();
                if (presents.Count == 0)
                {
                    return Failure("There are no active presents to process.");
                }

                var existingWinners = await dbContext.Winner
                    .Where(winner => winner.LotteryId == lotteryId &&
                                     winner.IsActive &&
                                     !winner.IsDeleted)
                    .ToListAsync();
                var winnersToAdd = new List<Winner>();

                foreach (var present in presents)
                {
                    if (present.Quantity <= 0)
                    {
                        return Failure($"Lottery processing stopped: present '{present.Name}' has no configured winners.");
                    }

                    var presentWinners = existingWinners
                        .Where(winner => winner.PresentId == present.Id)
                        .ToList();
                    var remainingWinnerCount = present.Quantity - presentWinners.Count;
                    if (remainingWinnerCount <= 0)
                    {
                        continue;
                    }

                    var alreadySelectedCardIds = presentWinners
                        .Select(winner => winner.CardId)
                        .ToHashSet();
                    var eligibleCards = await dbContext.Card
                        .Where(card => card.PresentId == present.Id &&
                                       card.IsPaid &&
                                       card.IsActive &&
                                       !card.IsDeleted &&
                                       !alreadySelectedCardIds.Contains(card.Id))
                        .OrderBy(card => card.Id)
                        .ToListAsync();
                    if (eligibleCards.Count < remainingWinnerCount)
                    {
                        logger.LogWarning(
                            "Annual lottery {LotteryId} cannot process present {PresentId}: {EligibleCount} eligible paid cards remain for {RequiredWinnerCount} winners.",
                            lotteryId,
                            present.Id,
                            eligibleCards.Count,
                            remainingWinnerCount);
                        return Failure(
                            $"Lottery processing stopped: present '{present.Name}' requires {remainingWinnerCount} more distinct paid cards.");
                    }

                    ShuffleFirst(eligibleCards, remainingWinnerCount);
                    winnersToAdd.AddRange(eligibleCards
                        .Take(remainingWinnerCount)
                        .Select(card => CreateWinner(present.Id, card.Id, lotteryId)));
                }

                var now = DateTime.UtcNow;
                dbContext.Winner.AddRange(winnersToAdd);
                lottery.IsMadeOut = true;
                lottery.UpdatedAt = now;
                await dbContext.SaveChangesAsync();
                var persistedWinners = await dbContext.Winner
                    .AsNoTracking()
                    .Where(winner => winner.LotteryId == lotteryId &&
                                     winner.IsActive &&
                                     !winner.IsDeleted)
                    .OrderBy(winner => winner.PresentId)
                    .ThenBy(winner => winner.Id)
                    .ToListAsync();
                if (transaction != null)
                {
                    await transaction.CommitAsync();
                }

                logger.LogInformation(
                    "Annual lottery {LotteryId} processed for {PresentCount} presents with {WinnerCount} winners.",
                    lotteryId,
                    presents.Count,
                    persistedWinners.Count);
                return new Result<Winner>
                {
                    Success = true,
                    Message = "Lottery processed successfully.",
                    Data = persistedWinners
                };
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process annual lottery {LotteryId}.", lotteryId);
                return Failure("Unable to process the lottery. Please try again.");
            }
        }

        private async Task<bool> AreAllActivePresentsDrawnAsync(int lotteryId)
        {
            var presents = await dbContext.Present
                .Where(present => present.IsActive && !present.IsDeleted)
                .Select(present => new { present.Id, present.Quantity })
                .ToListAsync();
            foreach (var present in presents)
            {
                var winnerCount = await dbContext.Winner.CountAsync(winner =>
                    winner.LotteryId == lotteryId &&
                    winner.PresentId == present.Id &&
                    winner.IsActive &&
                    !winner.IsDeleted);
                if (winnerCount < present.Quantity)
                {
                    return false;
                }
            }
            return presents.Count > 0;
        }

        private static void ShuffleFirst<T>(IList<T> items, int count)
        {
            for (var index = 0; index < count; index++)
            {
                var selectedIndex = Random.Shared.Next(index, items.Count);
                (items[index], items[selectedIndex]) = (items[selectedIndex], items[index]);
            }
        }

        private static Winner CreateWinner(int presentId, int cardId, int lotteryId) => new()
        {
            PresentId = presentId,
            CardId = cardId,
            LotteryId = lotteryId,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 0
        };

        private static Result<Winner> Failure(string message) => new()
        {
            Success = false,
            Message = message,
            Data = null
        };

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