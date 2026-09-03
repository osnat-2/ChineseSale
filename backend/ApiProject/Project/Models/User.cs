namespace Project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime RegisterationTime { get; set; } = DateTime.Now;
        public string Role { get; set; } = "User";
        public bool IsActive { get; set; } = true;
    }
}