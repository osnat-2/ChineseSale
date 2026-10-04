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
    public class CategoryDal : ICategoryDal
    {
        private readonly AppDBContext dbContext;  // משתנה שמייצג את מסד הנתונים
        private readonly ILogger<CategoryDal> logger; // אובייקט ללוגים

        // קונסטרוקטור - תפקידו לחבר את ה-DBContext וה-Logger למחלקה
        public CategoryDal(AppDBContext dbContext, ILogger<CategoryDal> logger)
        {
            this.dbContext = dbContext;  // מאתחלים את ה-DBContext
            this.logger = logger;        // מאתחלים את ה-Logger
        }

        // Get all active categories
        public async Task<Result<Category>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            try
            {
                var query = dbContext.Category.AsQueryable();
                
                // Filter to active categories only, unless explicitly requested otherwise
                if (!includeInactive)
                {
                    query = query.Where(c => c.IsActive == true);
                }

                var categories = await query.ToListAsync();

                logger.LogInformation("Fetched all categories successfully.");

                return new Result<Category>
                {
                    Success = true,
                    Message = "Fetched all categories successfully.",
                    Data = categories
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all categories.");
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Error fetching all categories: {ex.Message}",
                    Data = Enumerable.Empty<Category>()
                };
            }
        }

        // Get category by ID
        public async Task<Result<Category>> GetCategoryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    };
                }

                var category = await dbContext.Category.FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    logger.LogWarning($"Category with ID {id} not found.");
                    return new Result<Category>
                    {
                        Success = false,
                        Message = $"Category with ID {id} not found.",
                        Data = null
                    };
                }

                logger.LogInformation($"Fetched category with ID {id} successfully.");

                return new Result<Category>
                {
                    Success = true,
                    Message = "Category fetched successfully.",
                    Data = new List<Category> { category }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching category with ID {id}.");
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Error fetching category: {ex.Message}",
                    Data = null
                };
            }
        }

        // Check if category name exists (for duplicate checking)
        public async Task<Result<bool>> CategoryNameExistsAsync(string name, int? excludeCategoryId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new Result<bool>
                    {
                        Success = true,
                        Message = "Invalid name provided.",
                        Data = new List<bool> { false }
                    };
                }

                var query = dbContext.Category.Where(c => c.Name.ToLower() == name.ToLower());

                if (excludeCategoryId.HasValue)
                {
                    query = query.Where(c => c.Id != excludeCategoryId.Value);
                }

                var exists = await query.AnyAsync();

                return new Result<bool>
                {
                    Success = true,
                    Message = exists ? "Category name already exists" : "Category name is unique",
                    Data = new List<bool> { exists }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking if category name '{name}' exists.");
                return new Result<bool>
                {
                    Success = false,
                    Message = $"Error checking duplicate name: {ex.Message}",
                    Data = new List<bool> { false }
                };
            }
        }

        // Create new category
        public async Task<Result<Category>> CreateCategoryAsync(Category category)
        {
            try
            {
                if (category == null || string.IsNullOrWhiteSpace(category.Name))
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = "Category name is required.",
                        Data = null
                    };
                }

                // Check for duplicate name
                var nameExists = await dbContext.Category
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (nameExists)
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = $"Category with name '{category.Name}' already exists.",
                        Data = null
                    };
                }

                await dbContext.Category.AddAsync(category);
                await dbContext.SaveChangesAsync();

                logger.LogInformation($"Category '{category.Name}' created successfully.");

                return new Result<Category>
                {
                    Success = true,
                    Message = $"Category '{category.Name}' created successfully.",
                    Data = new List<Category> { category }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating category.");
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Error creating category: {ex.Message}",
                    Data = null
                };
            }
        }

        // Update existing category
        public async Task<Result<Category>> UpdateCategoryAsync(Category category)
        {
            try
            {
                if (category == null || category.Id <= 0)
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(category.Name))
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = "Category name is required.",
                        Data = null
                    };
                }

                var existingCategory = await dbContext.Category.FindAsync(category.Id);

                if (existingCategory == null)
                {
                    logger.LogWarning($"Category with ID {category.Id} not found.");
                    return new Result<Category>
                    {
                        Success = false,
                        Message = $"Category with ID {category.Id} not found.",
                        Data = null
                    };
                }

                // Check for duplicate name (excluding current category)
                var nameExists = await dbContext.Category
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id);

                if (nameExists)
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = $"Another category with name '{category.Name}' already exists.",
                        Data = null
                    };
                }

                existingCategory.Name = category.Name;
                existingCategory.IsActive = category.IsActive;

                dbContext.Category.Update(existingCategory);
                await dbContext.SaveChangesAsync();

                logger.LogInformation($"Category '{category.Name}' updated successfully.");

                return new Result<Category>
                {
                    Success = true,
                    Message = $"Category '{category.Name}' updated successfully.",
                    Data = new List<Category> { existingCategory }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating category.");
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Error updating category: {ex.Message}",
                    Data = null
                };
            }
        }

        // Soft delete - mark category as inactive
        public async Task<Result<Category>> DeleteCategoryAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    };
                }

                var category = await dbContext.Category.FindAsync(id);

                if (category == null)
                {
                    logger.LogWarning($"Category with ID {id} not found.");
                    return new Result<Category>
                    {
                        Success = false,
                        Message = $"Category with ID {id} not found.",
                        Data = null
                    };
                }

                // Soft delete: retain the record for audit/history while excluding it from normal queries.
                category.IsActive = false;
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
                dbContext.Category.Update(category);
                await dbContext.SaveChangesAsync();

                logger.LogInformation($"Category with ID {id} deleted successfully (soft delete).");

                return new Result<Category>
                {
                    Success = true,
                    Message = $"Category with ID {id} deleted successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting category with ID {id}.");
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Error deleting category: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
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
        //public async Task<Result<Card>> PurchaseCardAsync(Card card)
        //{
        //    try
        //    {
        //        // Prevent purchase if a winner has already been drawn for this present
        //        var raffleExists = await dbContext.Winner.AnyAsync(w => w.PresentId == card.PresentId);
        //        if (raffleExists)
        //        {
        //            return new Result<Card>
        //            {
        //                Success = false,
        //                Message = "Cannot purchase ticket: raffle already completed for this present.",
        //                Data = null
        //            };
        //        }

        //        dbContext.Card.Add(card);
        //        await dbContext.SaveChangesAsync();  // שמירה במסד הנתונים

        //        return new Result<Card>
        //        {
        //            Success = true,
        //            Message = "New card created successfully.",
        //            Data = null  // לא נדרש להחזיר נתונים נוספים
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error occurred while purchasing a card.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "An error occurred while purchasing the card. Please try again later.",
        //            Data = null
        //        };
        //    }
        //}

        //public async Task<Result<Card>> ProcessPaymentForUserAsync(int userId)
        //{
        //    try
        //    {
        //        // 1. חפש את כל הכרטיסים עבור המשתמש הספציפי שלא שולם עדיין
        //        var userCards = await dbContext.Card
        //            .Where(c => c.UserId == userId && c.IsPaid == false)  // כרטיסים שלא שולם עדיינם
        //            .ToListAsync();

        //        // 2. אם לא נמצאו כרטיסים, פשוט סיים את הפונקציה
        //        if (!userCards.Any())
        //        {
        //            return new Result<Card>
        //            {
        //                Success = false,
        //                Message = "No unpaid cards found for this user.",
        //                Data = null  // לא נדרש להחזיר נתונים נוספים
        //            };
        //        }

        //        // 3. עדכון כל הכרטיסים ל-"שולם"
        //        foreach (var card in userCards)
        //        {
        //            card.IsPaid = true;  // עדכון הסטטוס של הכרטיס לשולם
        //        }

        //        dbContext.Card.UpdateRange(userCards);  // עדכון כל הכרטיסים יחד
        //        await dbContext.SaveChangesAsync();  // שמירה במסד הנתונים

        //        return new Result<Card>
        //        {
        //            Success = true,
        //            Message = "Payment processed successfully for the user.",
        //            Data = null  // לא נדרש להחזיר נתונים נוספים
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error occurred while processing payments for the user.");
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "An error occurred while processing the payment. Please try again later.",
        //            Data = null
        //        };
        //    }
        //}
//    }
//}