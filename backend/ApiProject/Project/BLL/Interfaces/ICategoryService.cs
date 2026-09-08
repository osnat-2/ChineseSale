using Project.Dto;
using Project.Models;

namespace Project.Bll.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<Category>> GetAllCategoriesAsync(bool includeInactive = false);
        Task<Result<Category>> GetCategoryByIdAsync(int id);
        Task<Result<Category>> CreateCategoryAsync(CategoryDto categoryDto);
        Task<Result<Category>> UpdateCategoryAsync(int id, CategoryDto categoryDto);
        Task<Result<Category>> DeleteCategoryAsync(int id);
    }
}
