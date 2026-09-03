namespace Project.Models
{
    public class Lottery
    {
        public int Id { get; set; }
        public DateOnly Time { get; set; }
        public bool IsMadeOut { get; set; }
    }
}
