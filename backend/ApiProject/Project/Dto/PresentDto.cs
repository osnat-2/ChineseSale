using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class PresentDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int DonorId { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }

        [Required]
        [Url]
        [RegularExpression(@"^https?://", ErrorMessage = "Image URL must use HTTP or HTTPS.")]
        [StringLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [Range(0, int.MaxValue)]
        public int Price { get; set; } = 10;
    }
}