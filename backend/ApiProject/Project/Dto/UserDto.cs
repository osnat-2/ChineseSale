using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class UserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20, MinimumLength = 9)]
        [RegularExpression(@"^(?=.*[0-9])[0-9-]+$")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(72, MinimumLength = 4)]
        public string Password { get; set; } = string.Empty;
    }
}