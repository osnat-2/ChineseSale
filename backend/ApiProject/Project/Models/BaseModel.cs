using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class BaseModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; }
    }
}