using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.BLL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.Controllers
{
    [ApiController]
    [Route("api/category")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all active categories (optionally include inactive)
        /// </summary>
        /// <param name="includeInactive">Include inactive categories (admin only)</param>
        /// <returns>List of categories</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<Result<Category>>> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync(includeInactive);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Result<Category>
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Get a single category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<Category>>> GetCategoryById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    });
                }

                var result = await _categoryService.GetCategoryByIdAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Result<Category>
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Create a new category (Admin only)
        /// </summary>
        /// <param name="categoryDto">Category data</param>
        /// <returns>Created category</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<Category>>> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            try
            {
                if (categoryDto == null)
                {
                    return BadRequest(new Result<Category>
                    {
                        Success = false,
                        Message = "Category data is required.",
                        Data = null
                    });
                }

                var result = await _categoryService.CreateCategoryAsync(categoryDto);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data?.FirstOrDefault()?.Id }, result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Result<Category>
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Update an existing category (Admin only)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <param name="categoryDto">Updated category data</param>
        /// <returns>Updated category</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<Category>>> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    });
                }

                if (categoryDto == null)
                {
                    return BadRequest(new Result<Category>
                    {
                        Success = false,
                        Message = "Category data is required.",
                        Data = null
                    });
                }

                var result = await _categoryService.UpdateCategoryAsync(id, categoryDto);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Result<Category>
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Delete a category by ID (soft delete - marks as inactive)
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Result<Category>>> DeleteCategory(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new Result<Category>
                    {
                        Success = false,
                        Message = "Invalid category ID.",
                        Data = null
                    });
                }

                var result = await _categoryService.DeleteCategoryAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Result<Category>
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}