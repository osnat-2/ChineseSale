using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class LotteryDto
    {
        [Required]
        public DateOnly Time { get; set; }
    }
}