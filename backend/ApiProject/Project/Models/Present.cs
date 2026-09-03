using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Present : BaseModel
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int DonorId { get; set; }
        public User Donor { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;
        [Required]
        [DefaultValue(10)]
        public int Price { get; set; } = 10;
    }
}