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
    public class CardDal : ICardDal
    {
        private readonly AppDBContext dbContext;  // משתנה שמייצג את מסד הנתונים
        private readonly ILogger<CardDal> logger; // אובייקט ללוגים

        // קונסטרוקטור - תפקידו לחבר את ה-DBContext וה-Logger למחלקה
        public CardDal(AppDBContext dbContext, ILogger<CardDal> logger)
        {
            this.dbContext = dbContext;  // מאתחלים את ה-DBContext
            this.logger = logger;        // מאתחלים את ה-Logger
        }


        //// צפייה בפרטי הרוכשים עבור כל מתנה
        //public async Task<Result<User>> GetAllCardBuyersAsync()
        //{
        //    try
        //    {
        //        // מביאים את כל הכרטיסים ומחזירים את המידע על הרוכשים (Users)
        //        var buyers = await dbContext.Card
        //            .Where(c => c.IsPaid == true)
        //            .Include(c => c.User)  // טוענים את המידע על הרוכש (User)
        //            .Select(c => c.User)  // מחזירים את המידע על הרוכש לכל כרטיס
        //            .Distinct()  // מבטיחים שלא יהיו כפילויות של רוכשים, גם אם רכשו מספר כרטיסים
        //            .ToListAsync();  // מחזירים את התוצאות בצורה אסינכרונית

        //        logger.LogInformation("Fetched all card buyers without duplicates.");

        //        return new Result<User>
        //        {
        //            Success = true,
        //            Message = "Fetched all card buyers successfully.",
        //            Data = buyers  // מחזירים את פרטי כל הרוכשים (בלי כפילויות)
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // במקרה של שגיאה, נרשום אותה בלוג
        //        logger.LogError(ex, "Error fetching all card buyers.");
        //        return new Result<User>
        //        {
        //            Success = false,
        //            Message = "Error fetching all card buyers: " + ex.Message,
        //            Data = Enumerable.Empty<User>()  // מחזירים רשימה ריקה במקרה של שגיאה
        //        };
        //    }
        //}

        //// צפייה בכל הכרטיסים עבור כל מתנה
        //public async Task<Result<Card>> GetCardsByAllPresentsAsync()
        //{
        //    try
        //    {
        //        // מביאים את כל הכרטיסים השולומים, מקובצים לפי כל מתנה
        //        var cardsByPresent = await dbContext.Card
        //            .Where(c => c.IsPaid == true)
        //            .OrderBy(c => c.PresentId)
        //            .ToListAsync();

        //        logger.LogInformation("Fetched cards for all presents.");

        //        return new Result<Card>
        //        {
        //            Success = true,
        //            Message = "Successfully fetched cards for all presents.",
        //            Data = cardsByPresent
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching cards for all presents.");

        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = $"Error fetching cards for all presents: {ex.Message}",
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //}

        //// מיון רכישות לפי מחיר המתנה
        //public async Task<Result<Card>> GetMostExpensiveCardsAsync()
        //{
        //    try
        //    {
        //        var mostExpensiveCards = await dbContext.Card
        //            .OrderByDescending(c => c.Present.Price)  // מיון לפי המחיר של המתנה (היקר ביותר)
        //            .ToListAsync();  // מחזירים את כל הכרטיסים ממוינים

        //        if (mostExpensiveCards.Any())
        //        {
        //            logger.LogInformation($"Fetched most expensive cards.");
        //            return new Result<Card>
        //            {
        //                Success = true,
        //                Message = "Fetched most expensive cards successfully.",
        //                Data = mostExpensiveCards
        //            };
        //        }
        //        else
        //        {
        //            logger.LogWarning("No cards found.");
        //            return new Result<Card>
        //            {
        //                Success = false,
        //                Message = "No cards found.",
        //                Data = Enumerable.Empty<Card>()
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching most expensive cards.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Error fetching most expensive cards: " + ex.Message,
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //}

        //// מיון רכישות לפי המתנה הנרכשת ביותר
        //public async Task<Result<Card>> GetCardsByQuantityAsync()
        //{
        //    try
        //    {
        //        // קבוצת כרטיסים לפי PresentId וממיינים לפי הכמות הכוללת של כרטיסים שנרכשו
        //        var cards = await dbContext.Card
        //            .Where(c => c.IsPaid == true)
        //            .GroupBy(c => c.PresentId)  // קבוצת כרטיסים לפי PresentId
        //            .OrderByDescending(group => group.Sum(c => 1))  // מיון לפי סך הכרטיסים שנרכשו  ???????????good
        //            .Select(group => group.First())  // מחזירים את הכרטיס הראשון מכל קבוצה
        //            .ToListAsync();  // מחזירים את התוצאות כ-list

        //        logger.LogInformation("Fetched cards sorted by quantity.");

        //        return new Result<Card>
        //        {
        //            Success = true,
        //            Message = "Fetched cards sorted by quantity successfully.",
        //            Data = cards  // מחזירים את רשימת הכרטיסים הממוינת לפי כמות
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // במקרה של שגיאה, נרשום אותה בלוג
        //        logger.LogError(ex, "Error fetching cards by quantity.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Error fetching cards by quantity: " + ex.Message,
        //            Data = Enumerable.Empty<Card>()  // מחזירים רשימה ריקה במקרה של שגיאה
        //        };
        //    }
        //}

        //// החזרת כל הכרטיסים של משתמש ספציפי
        //public async Task<Result<Card>> GetCardsByUserAsync(int userId)
        //{
        //    try
        //    {
        //        var userCards = await dbContext.Card
        //            .Where(c => c.UserId == userId && c.IsPaid == true)  // מחפשים כרטיסים ששייכים למשתמש הספציפי וששולמו
        //            .Include(c => c.Present)
        //            .ToListAsync();

        //        if (userCards.Any())
        //        {
        //            logger.LogInformation($"Fetched cards for user {userId}.");
        //            return new Result<Card>
        //            {
        //                Success = true,
        //                Message = "Fetched cards for user successfully.",
        //                Data = userCards
        //            };
        //        }

        //        logger.LogWarning($"No cards found for user {userId}.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "No cards found for this user.",
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Error fetching cards for user {userId}.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Error fetching cards for user: " + ex.Message,
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //}

        //// החזרת כל הכרטיסים שלא שולמו
        //public async Task<Result<Card>> GetUnpaidCardsAsync(int userId)
        //{
        //    try
        //    {
        //        var unpaidCards = await dbContext.Card
        //            .Where(c => c.UserId == userId && c.IsPaid == false)
        //            .Include(c => c.Present)
        //            .ToListAsync();

        //        if (unpaidCards.Any())
        //        {
        //            logger.LogInformation($"Fetched unpaid cards for user {userId}.");
        //            return new Result<Card>
        //            {
        //                Success = true,
        //                Message = "Fetched unpaid cards for user successfully.",
        //                Data = unpaidCards
        //            };
        //        }

        //        logger.LogWarning($"No unpaid cards found for user {userId}.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "No unpaid cards found for this user.",
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error fetching unpaid cards.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Error fetching unpaid cards: " + ex.Message,
        //            Data = Enumerable.Empty<Card>()
        //        };
        //    }
        //}

        //// מחיקת כרטיס - רק אם לא שולם
        //public async Task<Result<Card>> DeleteCardAsync(int cardId)
        //{
        //    try
        //    {
        //        // חפש את הכרטיס לפי מזהה
        //        var card = await dbContext.Card
        //            .FirstOrDefaultAsync(c => c.Id == cardId);  // מחפש כרטיס לפי מזהה

        //        // אם לא נמצא כרטיס או שהכרטיס כבר שולם, מחזירים false
        //        if (card == null || card.IsPaid == true)
        //        {
        //            return new Result<Card>
        //            {
        //                Success = false,
        //                Message = "Card not found or already paid, cannot delete.",
        //                Data = null
        //            };
        //        }

        //        // אם הכרטיס לא שולם, מבצעים את המחיקה
        //        dbContext.Card.Remove(card);
        //        await dbContext.SaveChangesAsync();  // שומרים את השינויים במסד הנתונים

        //        return new Result<Card>
        //        {
        //            Success = true,
        //            Message = "Card deleted successfully.",
        //            Data = null  // ניתן להחזיר נתונים נוספים במקרה הצורך
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Error occurred while deleting card with id {cardId}.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "An error occurred while deleting the card. Please try again later.",
        //            Data = null
        //        };
        //    }
        //}
        public async Task<Result<Card>> AddCard(Card card)
        {
            return await AddCards(new[] { card });
        }

        public async Task<Result<Card>> AddCards(IReadOnlyCollection<Card> cards)
        {
            try
            {
                if (cards.Count == 0 || cards.Select(card => card.PresentId).Distinct().Count() != 1)
                {
                    return new Result<Card> { Success = false, Message = "Cards must belong to one present.", Data = null };
                }

                var presentId = cards.First().PresentId;
                var present = await dbContext.Present.FirstOrDefaultAsync(p => p.Id == presentId && p.IsActive);
                if (present == null)
                {
                    return new Result<Card> { Success = false, Message = "Present is unavailable.", Data = null };
                }

                // Prevent purchase if a winner has already been drawn for this present
                var raffleExists = await dbContext.Winner.AnyAsync(w => w.PresentId == presentId);
                if (raffleExists)
                {
                    return new Result<Card>
                    {
                        Success = false,
                        Message = "Cannot purchase ticket: raffle already completed for this present.",
                        Data = null
                    };
                }

                dbContext.Card.AddRange(cards);
                await dbContext.SaveChangesAsync();

                return new Result<Card>
                {
                    Success = true,
                    Message = cards.Count == 1
                        ? "New card created successfully."
                        : $"{cards.Count} cards created successfully.",
                    Data = cards.ToArray()
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while purchasing a card.");
                return new Result<Card>
                {
                    Success = false,
                    Message = "An error occurred while purchasing the card. Please try again later.",
                    Data = null
                };
            }
        }

        public async Task<Result<Card>> GetCardsByUserAsync(int userId, bool? isPaid = null)
        {
            var query = dbContext.Card
                .Include(card => card.Present)
                .Where(card => card.CreatedBy == userId && card.IsActive && !card.IsDeleted);
            if (isPaid.HasValue)
            {
                query = query.Where(card => card.IsPaid == isPaid.Value);
            }

            var cards = await query.OrderByDescending(card => card.CreatedAt).ToListAsync();
            return new Result<Card> { Success = true, Message = "Cards loaded successfully.", Data = cards };
        }

        public async Task<Result<Card>> DeleteCardAsync(int cardId, int userId)
        {
            var card = await dbContext.Card.FirstOrDefaultAsync(item => item.Id == cardId && item.CreatedBy == userId);
            if (card == null)
            {
                return new Result<Card> { Success = false, Message = "Card not found.", Data = null };
            }
            if (card.IsPaid)
            {
                return new Result<Card> { Success = false, Message = "Paid cards cannot be removed.", Data = null };
            }

            card.IsDeleted = true;
            card.IsActive = false;
            card.DeletedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return new Result<Card> { Success = true, Message = "Card removed successfully.", Data = new[] { card } };
        }

        public async Task<Result<Card>> ProcessDevelopmentPaymentAsync(int userId, decimal amount)
        {
            await using var transaction = dbContext.Database.IsRelational()
                ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                : null;

            var cards = await dbContext.Card
                .Include(card => card.Present)
                .Where(card =>
                    card.CreatedBy == userId &&
                    card.IsActive &&
                    !card.IsDeleted &&
                    card.Present.IsActive &&
                    !card.Present.IsDeleted)
                .ToListAsync();
            if (cards.Count == 0)
            {
                return new Result<Card> { Success = false, Message = "No cards found for this user.", Data = null };
            }

            var unpaidCards = cards.Where(card => !card.IsPaid).ToList();
            if (unpaidCards.Count == 0)
            {
                return new Result<Card>
                {
                    Success = true,
                    Message = "All cards have already been paid.",
                    Data = cards
                };
            }

            var expectedAmount = unpaidCards.Sum(card => (decimal)card.Present.Price);
            if (amount != expectedAmount)
            {
                return new Result<Card>
                {
                    Success = false,
                    Message = "The submitted amount does not match the current unpaid-card total.",
                    Data = null
                };
            }

            var now = DateTime.UtcNow;
            foreach (var card in unpaidCards)
            {
                card.IsPaid = true;
                card.UpdatedAt = now;
            }

            await dbContext.SaveChangesAsync();
            if (transaction != null)
            {
                await transaction.CommitAsync();
            }
            return new Result<Card>
            {
                Success = true,
                Message = "Development payment processed successfully.",
                Data = unpaidCards
            };
        }
    }
}