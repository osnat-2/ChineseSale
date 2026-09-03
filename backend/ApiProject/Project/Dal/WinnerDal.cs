using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Models;
using Project.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Project.Dal
{
    public class WinnerDal : IWinnerDal
    {
        private readonly AppDBContext dbContext;  // משתנה שמייצג את מסד הנתונים
        private readonly ILogger<WinnerDal> logger; // אובייקט ללוגים

        // קונסטרוקטור - מאתחל את ה-DBContext ו-Logger
        public WinnerDal(AppDBContext dbContext, ILogger<WinnerDal> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        // פונקציה שמגרילה זוכה עבור מתנה ספציפית
        public async Task<Result<Winner>> DrawWinnerForPresentAsync(int presentId)
        {
            Winner winnerInfo = null;

            try
            {
                // מחפשים את המתנה לפי presentId
                var present = await dbContext.Present
                    .FirstOrDefaultAsync(p => p.Id == presentId);

                if (present == null)
                {
                    logger.LogWarning($"Present with id {presentId} not found.");
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = $"Present with id {presentId} not found.",
                        Data = null
                    };
                }

                // בוחרים את כל הרוכשים ששילמו עבור כרטיסים למתנה הספציפית
                var buyers = await dbContext.Card
                    .Where(c => c.PresentId == presentId && c.IsPaid == true)
                    .Select(c => c.User)  // מחזירים את המידע של הרוכש
                    .ToListAsync();

                if (!buyers.Any())
                {
                    logger.LogWarning($"No paid buyers found for present with id {presentId}.");
                    return new Result<Winner>
                    {
                        Success = false,
                        Message = $"No paid buyers found for present with id {presentId}.",
                        Data = null
                    };
                }

                // לבחור אקראי מתוך הרשימה של הרוכשים
                Random random = new Random();
                var winner = buyers[random.Next(buyers.Count)];  // בוחרים אקראי מתוך הרשימה

                if (winner != null)
                {
                    // יוצרים את אובייקט הזוכה
                    winnerInfo = new Winner
                    {
                        PresentId = presentId,
                        UserId = winner.Id,  // אין צורך ב-ToString() כאן
                        LotteryId = DateOnly.FromDateTime(DateTime.Now)
                    };

                    // שליחת מייל לזוכה
                    // await SendWinnerEmailAsync(winner, present);
                    logger.LogInformation($"Winner for present with id {presentId} is {winner.Name}.");

                    // מחזירים את הזוכה ב-Result
                    return new Result<Winner>
                    {
                        Success = true,
                        Message = "Winner drawn successfully.",
                        Data = new List<Winner> { winnerInfo } // מחזירים את פרטי הזוכה בתוך List
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while drawing winner for present with id {presentId}.");
                return new Result<Winner>
                {
                    Success = false,
                    Message = "An error occurred while drawing the winner. Please try again later.",
                    Data = null
                };
            }

            // במקרה שאין זוכה, מחזירים תוצאה שלילית
            return new Result<Winner>
            {
                Success = false,
                Message = "No winner found.",
                Data = null
            };
        }


        //??? for all the present not to a specific!!
        public async Task<Result<Dictionary<Present, List<User>>>> GetPresentsWithUsersAsync()
        {
            try
            {
                // שישומ מידע שהכרטיס שולם
                var winners = await dbContext.Winner
                    .ToListAsync();

                // יצירת מילון שבו המפתח הוא ה-`Present` והערך הוא רשימת משתמשים (Users)
                var presentsWithUsers = winners
                    .GroupBy(w => w.PresentId);  // קבוצות לפי האובייקט Present
                    //.ToDictionary(g => g.Key, g => g.Select(w => w.User).ToList());  // ממיר לכל Present רשימה של Users

                // החזרת התוצאה בתוך Result<Dictionary<Present, List<User>>>:
                return new Result<Dictionary<Present, List<User>>>
                {
                    Success = true,
                    Message = "Presents with users fetched successfully.",
                    //Data = new List<Dictionary<Present, List<User>>> { presentsWithUsers }
                };
            }
            catch (Exception ex)
            {
                // לוג שגיאה
                logger.LogError(ex, "Error occurred while fetching presents with users.");

                // החזרת תוצאה עם שגיאה
                return new Result<Dictionary<Present, List<User>>>
                {
                    Success = false,
                    Message = "An error occurred while fetching presents with users.",
                    Data = null
                };
            }
        }

        // פונקציה לחישוב סך ההכנסות למכירה עבור כל המתנות
        public async Task<decimal> CalculateTotalIncomeForPresentAsync()
        {
            try
            {
                // חישוב סך ההכנסות לכל כרטיס ששולם
                var totalIncome = await dbContext.Card
                    .Where(c => c.IsPaid == true)
                    .SumAsync(c => c.Present.Price);

                logger.LogInformation($"Total income for all presents is {totalIncome:C}");
                return totalIncome;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error occurred while calculating total income for all presents.");
                return 0m;
            }
        }
    }
}