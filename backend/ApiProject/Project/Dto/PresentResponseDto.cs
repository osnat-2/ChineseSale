namespace Project.Dto;

public class PresentResponseDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int CreatedBy { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DonorId { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int Price { get; set; }
}
