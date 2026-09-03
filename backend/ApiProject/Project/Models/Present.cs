using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Present
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int DonorId { get; set; }
        public Donor Donor { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        [Required]
        public int Quantity { get; set; }
        [DefaultValue(10)]
        public int Price { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}