namespace WebApplication1.Data
{
    public record User
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; } = DateOnly.MinValue;
    }
}
