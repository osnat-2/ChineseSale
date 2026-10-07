using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class RoleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
