using AutoMapper;
using Project;
using Project.Bll;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class CategoryManagementTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCategory_PersistsRequestedActiveState(bool isActive)
    {
        var dal = new StubCategoryDal();
        var service = CreateService(dal);

        var result = await service.CreateCategoryAsync(new CategoryDto
        {
            Name = "Books",
            IsActive = isActive
        });

        Assert.True(result.Success);
        Assert.Equal(isActive, Assert.Single(dal.Created).IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateCategory_PersistsRequestedActiveState(bool isActive)
    {
        var dal = new StubCategoryDal();
        var service = CreateService(dal);

        var result = await service.UpdateCategoryAsync(4, new CategoryDto
        {
            Name = "Books",
            IsActive = isActive
        });

        Assert.True(result.Success);
        Assert.Equal(isActive, Assert.Single(dal.Updated).IsActive);
        Assert.Equal(4, dal.Updated[0].Id);
    }

    private static CategoryService CreateService(ICategoryDal dal) =>
        new(dal, new MapperConfiguration(configuration =>
            configuration.AddProfile<CategoryProfile>()).CreateMapper());

    private sealed class StubCategoryDal : ICategoryDal
    {
        public List<Category> Created { get; } = [];
        public List<Category> Updated { get; } = [];

        public Task<Result<Category>> GetAllCategoriesAsync(bool includeInactive = false) =>
            Task.FromResult(Success(Array.Empty<Category>()));

        public Task<Result<Category>> GetCategoryByIdAsync(int id) =>
            Task.FromResult(Success(new[]
            {
                new Category { Id = id, Name = "Books", IsActive = true }
            }));

        public Task<IReadOnlyDictionary<int, string>> GetCategoryNamesByIdsAsync(IReadOnlyCollection<int> ids) =>
            Task.FromResult<IReadOnlyDictionary<int, string>>(new Dictionary<int, string>());

        public Task<Result<bool>> CategoryNameExistsAsync(string name, int? excludeCategoryId = null) =>
            Task.FromResult(new Result<bool>
            {
                Success = true,
                Data = new[] { false }
            });

        public Task<Result<Category>> CreateCategoryAsync(Category category)
        {
            Created.Add(category);
            return Task.FromResult(Success(new[] { category }));
        }

        public Task<Result<Category>> UpdateCategoryAsync(Category category)
        {
            Updated.Add(category);
            return Task.FromResult(Success(new[] { category }));
        }

        public Task<Result<Category>> DeleteCategoryAsync(int id) =>
            Task.FromResult(Success(Array.Empty<Category>()));

        private static Result<Category> Success(IEnumerable<Category> categories) =>
            new()
            {
                Success = true,
                Data = categories
            };
    }
}
