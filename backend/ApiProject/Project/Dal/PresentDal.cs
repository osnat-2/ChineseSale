using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.DAL.Interfaces;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    // מחלקת ה-DAL עבור מתנות
    public class PresentDal : IPresentDal
    {
        private readonly AppDBContext dbContext;  // מייצג את מסד הנתונים
        private readonly ILogger<PresentDal> logger; // אובייקט לוגים לתיעוד פעולות

        // קונסטרוקטור - אתחול של ה-DBContext וה-Logger
        public PresentDal(AppDBContext dbContext, ILogger<PresentDal> logger)
        {
            this.dbContext = dbContext;  // אתחול ה-DBContext, מאפשר חיבור למסד הנתונים
            this.logger = logger;        // אתחול ה-Logger, מאפשר רישום פעולות
        }

        // Helper method to check if raffle has occurred
        private async Task<bool> HasRaffleOccurredAsync()
        {
            return await dbContext.Winner.AnyAsync();
        }

        // Check if present name exists (optionally exclude a specific present ID)
        public async Task<Result<bool>> PresentNameExistsAsync(string name, int? excludePresentId = null)
        {
            try
            {
                var query = dbContext.Present.Where(p => p.Name.ToLower() == name.ToLower());
                
                if (excludePresentId.HasValue)
                {
                    query = query.Where(p => p.Id != excludePresentId.Value);
                }

                var exists = await query.AnyAsync();
                
                return new Result<bool>
                {
                    Success = true,
                    Message = exists ? "Present name already exists" : "Present name is unique",
                    Data = new List<bool> { exists }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking if present name '{name}' exists.");
                return new Result<bool>
                {
                    Success = false,
                    Message = $"Error checking duplicate name: {ex.Message}",
                    Data = new List<bool> { false }
                };
            }
        }

        // צפייה ברשימת המתנות
        public async Task<Result<Present>> GetAllPresentsAsync(bool onlyActive = true)
        {
            try
            {
                // מבצעים חיפוש על טבלת המתנות, כולל המידע על התורם וקטגוריה (באמצעות Include)
                // Using AsNoTracking for better performance and to avoid circular references
                var query = dbContext.Present
                    .AsNoTracking()
                    .Include(p => p.Donor)  // טוענים את פרטי התורם לכל מתנה
                    .Include(p => p.Category)  // טוענים את פרטי הקטגוריה לכל מתנה
                    .AsQueryable();

                if (onlyActive)
                {
                    query = query.Where(p => p.IsActive);
                }

                var presents = await query.ToListAsync();  // מבצעים את החיפוש בצורה אסינכרונית

                // תיעוד בלוג שהפונקציה הצליחה
                logger.LogInformation("Fetched all presents successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched all presents successfully.",
                    Data = presents  // מחזירים את כל המתנות שנמצאו
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, "Error fetching all presents.");
                return new Result<Present>
                {
                    Success = false,
                    Message = "Error fetching all presents: " + ex.Message,
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }

        // Get present by ID
        public async Task<Result<Present>> GetPresentByIdAsync(int id)
        {
            try
            {
                var present = await dbContext.Present
                    .Include(p => p.Donor)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (present == null)
                {
                    logger.LogWarning($"Present with ID {id} not found.");
                    return new Result<Present>
                    {
                        Success = false,
                        Message = $"Present with ID {id} not found.",
                        Data = null
                    };
                }

                logger.LogInformation($"Fetched present with ID {id} successfully.");
                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched present successfully.",
                    Data = new List<Present> { present }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching present with ID {id}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error fetching present with ID {id}: {ex.Message}",
                    Data = Enumerable.Empty<Present>()
                };
            }
        }


        // הוספת מתנה
        public async Task<Result<Present>> AddPresentAsync(Present present)
        {
            try
            {
                // מוסיפים את המתנה החדשה למסד הנתונים
                await dbContext.Present.AddAsync(present);
                await dbContext.SaveChangesAsync();  // שומרים את השינויים במסד הנתונים

                // תיעוד בלוג על הצלחה בהוספת המתנה
                logger.LogInformation($"Present '{present.Name}' added successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Present '{present.Name}' added successfully.",
                    Data = null  // לא נדרש להחזיר נתונים נוספים
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, "Error adding present.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error adding present: {ex.Message}",
                    Data = null
                };
            }
        }


        // מחיקת מתנה (Soft Delete - marks as inactive instead of removing)
        public async Task<Result<Present>> DeletePresentAsync(int presentId)
        {
            try
            {
                // מחפשים את המתנה לפי ה-ID שלה
                var present = await dbContext.Present.FindAsync(presentId);
                if (present == null)
                {
                    // אם לא נמצאה מתנה עם ה-ID הזה, נרשום אזהרה בלוג
                    logger.LogWarning($"Present with ID {presentId} not found.");
                    return new Result<Present>
                    {
                        Success = false,
                        Message = $"Present with ID {presentId} not found.",
                        Data = null
                    };
                }

                // Soft Delete: Set IsActive to false instead of removing the record
                present.IsActive = false;
                dbContext.Present.Update(present);
                await dbContext.SaveChangesAsync();  // שומרים את השינויים במסד הנתונים

                // תיעוד בלוג על הצלחה במחיקה רכה של המתנה
                logger.LogInformation($"Present with ID {presentId} soft deleted successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Present with ID {presentId} deleted successfully.",
                    Data = null  // לא נדרש להחזיר נתונים נוספים
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, $"Error deleting present with ID {presentId}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error deleting present with ID {presentId}: {ex.Message}",
                    Data = null
                };
            }
        }


        // עדכון מתנה
        public async Task<Result<Present>> UpdatePresentAsync(Present present)
        {
            try
            {
                var p = dbContext.Present.FirstOrDefault(p => p.Id == present.Id);
                if (p == null)
                {
                    logger.LogWarning($"Present id {present.Id} doesn't exist.");
                    return new Result<Present>
                    {
                        Success = false,
                        Message = $"Present id {present.Id} doesn't exist.",
                        Data = null
                    };
                }

                p.Name = present.Name;
                p.DonorId = present.DonorId;
                p.Category = present.Category;
                p.Quantity = present.Quantity;
                p.Price = present.Price;
                p.Description = present.Description;
                // מעדכנים את פרטי המתנה במסד הנתונים
                dbContext.Present.Update(p);
                await dbContext.SaveChangesAsync();  // שומרים את השינויים

                // תיעוד בלוג על הצלחה בעדכון המתנה
                logger.LogInformation($"Present '{present.Name}' updated successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Present '{present.Name}' updated successfully.",
                    Data = null  // לא נדרש להחזיר נתונים נוספים
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, $"Error updating present with ID {present.Id}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error updating present with ID {present.Id}: {ex.Message}",
                    Data = null
                };
            }
        }

        // חיפוש מתנה לפי שם
        public async Task<Result<Present>> GetPresentsByNameAsync(string name)
        {
            try
            {
                var presents = await dbContext.Present
                    .Where(p => p.IsActive && p.Name.Contains(name))  // Exclude inactive presents
                    .Include(p => p.Donor)  // כולל את המידע על התורם
                    .ToListAsync();

                logger.LogInformation("Found presents by name");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Fetched presents matching name '{name}' successfully.",
                    Data = presents  // מחזירים את המתנות שנמצאו
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching presents by name {name}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error fetching presents by name {name}: {ex.Message}",
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }


        // חיפוש מתנה לפי שם תורם
        public async Task<Result<Present>> GetPresentsByDonorNameAsync(string donorName)
        {
            try
            {
                var presents = await dbContext.Present
                    .Where(p => p.IsActive && p.Donor.Name.Contains(donorName))  // Exclude inactive presents
                    .Include(p => p.Donor)  // כולל את המידע על התורם
                    .ToListAsync();

                logger.LogInformation("Found presents by donorName");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Fetched presents for donor with name '{donorName}' successfully.",
                    Data = presents  // מחזירים את המתנות שנמצאו
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching presents by donor name {donorName}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error fetching presents by donor name {donorName}: {ex.Message}",
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }


        // חיפוש מתנה לפי מספר רוכשים
        public async Task<Result<Present>> GetPresentsByBuyerCountAsync(int buyerCount)
        {
            try
            {
                // Fix N+1 query issue using GroupBy instead of Count in Where clause
                // Exclude inactive presents
                var presents = await dbContext.Present
                    .Where(p => p.IsActive)
                    .GroupJoin(
                        dbContext.Card,
                        p => p.Id,
                        c => c.PresentId,
                        (p, cards) => new { Present = p, CardCount = cards.Count() }
                    )
                    .Where(x => x.CardCount == buyerCount)
                    .Select(x => x.Present)
                    .Include(p => p.Donor)  // כולל את המידע על התורם
                    .ToListAsync();

                logger.LogInformation($"Found present by {buyerCount} buyers");

                return new Result<Present>
                {
                    Success = true,
                    Message = $"Fetched presents with {buyerCount} buyers successfully.",
                    Data = presents  // מחזירים את המתנות שנמצאו
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching presents by buyer count {buyerCount}.");
                return new Result<Present>
                {
                    Success = false,
                    Message = $"Error fetching presents by buyer count {buyerCount}: {ex.Message}",
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }

        // הוצאת כל המתנות מסודרות לפי מחיר
        public async Task<Result<Present>> GetPresentsByPriceAsync()
        {
            try
            {
                // מקבלים את כל המתנות מסודרות לפי מחיר בסדר עולה
                // Exclude inactive presents
                var presents = await dbContext.Present
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Price)  // מיון לפי מחיר
                    .Include(p => p.Donor)  // כולל את המידע על התורם
                    .ToListAsync();

                // תיעוד בלוג על הצלחה
                logger.LogInformation("Fetched all presents ordered by price successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched all presents ordered by price successfully.",
                    Data = presents  // מחזירים את המתנות מסודרות לפי מחיר
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, "Error fetching presents ordered by price.");
                return new Result<Present>
                {
                    Success = false,
                    Message = "Error fetching presents ordered by price: " + ex.Message,
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }

        // הוצאת כל המתנות מסודרות לפי קטגוריה
        public async Task<Result<Present>> GetPresentsByCategoryAsync()
        {
            try
            {
                // מקבלים את כל המתנות מסודרות לפי קטגוריה
                // Exclude inactive presents
                var presents = await dbContext.Present
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)  // מיון לפי קטגוריה
                    .Include(p => p.Donor)  // כולל את המידע על התורם
                    .ToListAsync();

                // תיעוד בלוג על הצלחה
                logger.LogInformation("Fetched all presents ordered by category successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched all presents ordered by category successfully.",
                    Data = presents  // מחזירים את המתנות מסודרות לפי קטגוריה
                };
            }
            catch (Exception ex)
            {
                // במקרה של שגיאה, נתעד את השגיאה בלוג
                logger.LogError(ex, "Error fetching presents ordered by category.");
                return new Result<Present>
                {
                    Success = false,
                    Message = "Error fetching presents ordered by category: " + ex.Message,
                    Data = Enumerable.Empty<Present>()  // מחזירים רשימה ריקה במקרה של שגיאה
                };
            }
        }

        // Get all presents along with winner info (if any) using existing Present model with Include
        public async Task<Result<Present>> GetAllPresentsWithWinnersAsync()
        {
            try
            {
                var presents = await dbContext.Present
                    .Include(p => p.Donor)
                    .ToListAsync();

                // Load winners for each present (no explicit Include, but Winner table has PresentId)
                var winnersByPresentId = await dbContext.Winner
                    .ToListAsync();

                logger.LogInformation("Fetched presents with winner info.");
                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched presents with winner info successfully.",
                    Data = presents
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching presents with winner info.");
                return new Result<Present>
                {
                    Success = false,
                    Message = "Error fetching presents with winner info: " + ex.Message,
                    Data = Enumerable.Empty<Present>()
                };
            }
        }
//*** End Patch

        // Get all presents without including related entities
        public async Task<Result<Present>> GetAllPresentsNoIncludeAsync()
        {
            try
            {
                var presents = await dbContext.Present
                    .ToListAsync();

                logger.LogInformation("Fetched all presents without includes successfully.");

                return new Result<Present>
                {
                    Success = true,
                    Message = "Fetched all presents without includes successfully.",
                    Data = presents
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching all presents without includes.");
                return new Result<Present>
                {
                    Success = false,
                    Message = "Error fetching all presents without includes: " + ex.Message,
                    Data = Enumerable.Empty<Present>()
                };
            }
        }

        //// 6. חיפוש מחיר כרטיס הגרלה למתנה מסוימת
        //public async Task<int?> GetTicketPriceAsync(int presentId)
        //{
        //    try
        //    {
        //        // מחפשים את המתנה לפי ה-ID שלה
        //        var present = await dbContext.Presents.FindAsync(presentId);

        //        if (present == null)
        //        {
        //            // אם המתנה לא נמצאה, נרשום אזהרה בלוג
        //            logger.LogWarning($"Present with ID {presentId} not found.");
        //            return null;  // אם המתנה לא נמצאה, מחזירים null
        //        }

        //        // תיעוד בלוג על הצלחה בהחזרת מחיר הכרטיס
        //        logger.LogInformation($"Fetched ticket price for present with ID {presentId}: {present.Price}");
        //        return present.Price;  // מחזירים את מחיר כרטיס ההגרלה של המתנה
        //    }
        //    catch (Exception ex)
        //    {
        //        // במקרה של שגיאה, נתעד את השגיאה בלוג
        //        logger.LogError(ex, $"Error fetching ticket price for present with ID {presentId}.");
        //        return null;  // מחזירים null במקרה של שגיאה
        //    }
        //}
    }
}