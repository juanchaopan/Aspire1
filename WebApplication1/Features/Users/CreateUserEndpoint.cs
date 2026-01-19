using Microsoft.AspNetCore.Mvc;
using Shared;
using System.Runtime.CompilerServices;
using WebApplication1.Data;

namespace WebApplication1.Features.Users
{
    public static class CreateUserEndpoint
    {
        public static async Task<IResult> CreateUser([FromBody] CreateUserRequest request, AppDbContext appDbContext, ILogger<Program> logger)
        {
            try
            {
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    DateOfBirth = request.DateOfBirth
                };

                appDbContext.Users.Add(user);
                await appDbContext.SaveChangesAsync();

                var response = new CreateUserResponse(user.Id, user.Name, user.Email, user.DateOfBirth);

                return Results.Created($"users/{user.Id}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating user");
                return Results.Problem("An error occurred while creating the user.");
            }
        }
    }
}
