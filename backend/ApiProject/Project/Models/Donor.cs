using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Donor
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CreatedBy { get; set; }
        public ICollection<Present> Presents { get; set; } = new List<Present>();
    }
}
