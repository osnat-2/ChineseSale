using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Bll;

public class PresentResponseMapper
{
    private readonly ICategoryDal _categoryDal;

    public PresentResponseMapper(ICategoryDal categoryDal)
    {
        _categoryDal = categoryDal;
    }

    public async Task<Result<PresentResponseDto>> MapPresentsAsync(Result<Present> result)
    {
        if (result.Data == null)
        {
            return new Result<PresentResponseDto>
            {
                Success = result.Success,
                Message = result.Message,
                Data = null
            };
        }

        var presents = result.Data.ToArray();
        var categoryNames = await _categoryDal.GetCategoryNamesByIdsAsync(
            presents.Select(present => present.CategoryId).Distinct().ToArray());

        return new Result<PresentResponseDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = presents.Select(present => MapPresent(present, categoryNames)).ToArray()
        };
    }

    public async Task<Result<CardResponseDto>> MapCardsAsync(Result<Card> result)
    {
        if (result.Data == null)
        {
            return new Result<CardResponseDto>
            {
                Success = result.Success,
                Message = result.Message,
                Data = null
            };
        }

        var cards = result.Data.ToArray();
        var categoryNames = await _categoryDal.GetCategoryNamesByIdsAsync(
            cards.Where(card => card.Present != null)
                .Select(card => card.Present.CategoryId)
                .Distinct()
                .ToArray());

        return new Result<CardResponseDto>
        {
            Success = result.Success,
            Message = result.Message,
            Data = cards.Select(card => new CardResponseDto
            {
                Id = card.Id,
                IsActive = card.IsActive,
                IsDeleted = card.IsDeleted,
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                DeletedAt = card.DeletedAt,
                CreatedBy = card.CreatedBy,
                PresentId = card.PresentId,
                Present = card.Present == null ? null : MapPresent(card.Present, categoryNames),
                IsPaid = card.IsPaid
            }).ToArray()
        };
    }

    private static PresentResponseDto MapPresent(
        Present present,
        IReadOnlyDictionary<int, string> categoryNames) =>
        new()
        {
            Id = present.Id,
            IsActive = present.IsActive,
            IsDeleted = present.IsDeleted,
            CreatedAt = present.CreatedAt,
            UpdatedAt = present.UpdatedAt,
            DeletedAt = present.DeletedAt,
            CreatedBy = present.CreatedBy,
            Name = present.Name,
            Description = present.Description,
            DonorId = present.DonorId,
            CategoryId = present.CategoryId,
            CategoryName = categoryNames.TryGetValue(present.CategoryId, out var name) ? name : null,
            ImageUrl = present.ImageUrl,
            Quantity = present.Quantity,
            Price = present.Price
        };
}
