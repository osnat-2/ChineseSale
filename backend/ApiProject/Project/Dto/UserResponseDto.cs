namespace Project.Dto
{
    public sealed class UserResponseDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Phone { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
}
