using AutoMapper;
using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;
using Project.Validators;

namespace Project.Bll
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryDal _categoryDal;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryDal categoryDal, IMapper mapper)
        {
            _categoryDal = categoryDal;
            _mapper = mapper;
        }

        // Get all active categories (or include inactive if specified)
        public async Task<Result<Category>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            return await _categoryDal.GetAllCategoriesAsync(includeInactive);
        }

        // Get single category by ID
        public async Task<Result<Category>> GetCategoryByIdAsync(int id)
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

            return await _categoryDal.GetCategoryByIdAsync(id);
        }

        // Create new category with validation
        public async Task<Result<Category>> CreateCategoryAsync(CategoryDto categoryDto)
        {
            if (categoryDto == null)
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category data is required.",
                    Data = null
                };
            }

            // Validate category name
            if (!Validator.ValidName(categoryDto.Name))
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category name is required and cannot be empty.",
                    Data = null
                };
            }

            // Additional validation: name length
            if (categoryDto.Name.Length > 100)
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category name cannot exceed 100 characters.",
                    Data = null
                };
            }

            // Check for duplicate name
            var duplicateCheck = await _categoryDal.CategoryNameExistsAsync(categoryDto.Name);
            if (duplicateCheck != null && duplicateCheck.Success && duplicateCheck.Data.FirstOrDefault())
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = $"A category with name '{categoryDto.Name}' already exists.",
                    Data = null
                };
            }

            // Map DTO to model
            var category = _mapper.Map<Category>(categoryDto);
            return await _categoryDal.CreateCategoryAsync(category);
        }

        // Update existing category with validation
        public async Task<Result<Category>> UpdateCategoryAsync(int id, CategoryDto categoryDto)
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

            if (categoryDto == null)
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category data is required.",
                    Data = null
                };
            }

            // Validate category name
            if (!Validator.ValidName(categoryDto.Name))
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category name is required and cannot be empty.",
                    Data = null
                };
            }

            // Additional validation: name length
            if (categoryDto.Name.Length > 100)
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = "Category name cannot exceed 100 characters.",
                    Data = null
                };
            }

            // Check if category exists
            var existingCategory = await _categoryDal.GetCategoryByIdAsync(id);
            if (existingCategory == null || !existingCategory.Success)
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Category with ID {id} not found.",
                    Data = null
                };
            }

            // Check for duplicate name (excluding current category)
            var duplicateCheck = await _categoryDal.CategoryNameExistsAsync(categoryDto.Name, id);
            if (duplicateCheck != null && duplicateCheck.Success && duplicateCheck.Data.FirstOrDefault())
            {
                return new Result<Category>
                {
                    Success = false,
                    Message = $"Another category with name '{categoryDto.Name}' already exists.",
                    Data = null
                };
            }

            // Map DTO to model and update ID
            var category = _mapper.Map<Category>(categoryDto);
            category.Id = id;
            return await _categoryDal.UpdateCategoryAsync(category);
        }

        // Delete category (soft delete - marks as inactive)
        public async Task<Result<Category>> DeleteCategoryAsync(int id)
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

            return await _categoryDal.DeleteCategoryAsync(id);
        }
    }
}