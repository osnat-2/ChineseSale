using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class CardDto
    {
        public const int MaximumQuantity = 100;

        [Range(1, int.MaxValue)]
        public int PresentId { get; set; }

        [Range(1, MaximumQuantity)]
        public int Quantity { get; set; } = 1;
    }
}