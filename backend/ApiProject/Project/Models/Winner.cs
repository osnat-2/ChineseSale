using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Winner : BaseModel
    {
        [Required]
        public int PresentId { get; set; }
        public Present Present { get; set; }
        [Required]
        public int CardId { get; set; }
        public Card Card { get; set; }
        [Required]
        public int LotteryId { get; set; }
        public Lottery Lottery { get; set; }
    }
}