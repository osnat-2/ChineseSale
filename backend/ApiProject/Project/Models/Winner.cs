using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Winner
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int PresentId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public int LotteryId { get; set; }
        public Lottery Lottery { get; set; }
    }
}