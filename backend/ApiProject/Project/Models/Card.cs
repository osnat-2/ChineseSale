using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Card : BaseModel
    {
        [Required]
        public int PresentId { get; set; }
        public Present Present { get; set; }
        [Required]
        public bool IsPaid { get; set; }
    }
}