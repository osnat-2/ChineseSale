using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Role : BaseModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}