using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class UserRole : BaseModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        [DefaultValue("")]
        [MaxLength(18)]
        [MinLength(4)]
        public string? Password { get; set; } = "";
        [Required]
        // [DefaultValue("User")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}