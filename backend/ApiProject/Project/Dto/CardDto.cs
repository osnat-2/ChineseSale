using System.ComponentModel.DataAnnotations;

namespace Project.Dto
{
    public class CardDto
    {
        [Range(1, int.MaxValue)]
        public int PresentId { get; set; }
    }
}