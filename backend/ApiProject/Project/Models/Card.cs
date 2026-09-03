using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Card
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int PresentId { get; set; }
        public Present Present { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        [Required]
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}