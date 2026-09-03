using Project.Models;

namespace Project.DAL.Interfaces
{
    public interface ICategoryDal
    {
        Task<Result<Category>> GetAllCategoriesAsync(bool includeInactive = false);
        Task<Result<Category>> GetCategoryByIdAsync(int id);
        Task<Result<bool>> CategoryNameExistsAsync(string name, int? excludeCategoryId = null);
        Task<Result<Category>> CreateCategoryAsync(Category category);
        Task<Result<Category>> UpdateCategoryAsync(Category category);
        Task<Result<Category>> DeleteCategoryAsync(int id);
    }
}
