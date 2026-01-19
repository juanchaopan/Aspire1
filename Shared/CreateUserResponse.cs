namespace Shared
{
    public record CreateUserResponse(Guid Id, string Name, string Email, DateOnly DateOfBirth);

}
