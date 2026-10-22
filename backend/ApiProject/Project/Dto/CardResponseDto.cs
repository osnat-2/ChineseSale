namespace Project.Dto;

public class CardResponseDto
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int CreatedBy { get; set; }
    public int PresentId { get; set; }
    public PresentResponseDto? Present { get; set; }
    public bool IsPaid { get; set; }
}
