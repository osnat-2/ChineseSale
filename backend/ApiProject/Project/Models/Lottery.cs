using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Lottery : BaseModel
    {
        [Required]
        public DateOnly Time { get; set; }
        [Required]
        public bool IsMadeOut { get; set; }
    }
}