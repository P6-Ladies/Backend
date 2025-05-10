// src\Endpoints\UserEndpoints.cs

using backend.Entities.Users;
using backend.Entities.Users.DTOs;
using backend.Mappings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace backend.Endpoints;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("users");

        // POST /users
        group.MapPost("/", async (CreateUserDTO newUser, UserManager<User> userManager) =>
        {
            if (string.IsNullOrWhiteSpace(newUser.Email) || !newUser.Email.Contains("@"))
                return Results.BadRequest("Invalid email");

            if (newUser.Password.Length < 8)
                return Results.BadRequest("Password too weak");
                
            User user = newUser.ToEntity();
            var result = await userManager.CreateAsync(user, newUser.Password);
            return result.Succeeded 
                ? Results.Created($"/users/{user.Id}", user.ToDTO()) 
                : Results.BadRequest();
        })
        .WithName("CreateUser")
        .WithTags("Users")
        .WithDescription("Creates a new user with the specified details.")
        .Accepts<CreateUserDTO>("application/json")
        .Produces<User>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);
        
        // Get /users/{id}
        group.MapGet("/{userId}", async (int userId, UserManager<User> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(user.ToDTO());
        })
        .WithName("GetUser")
        .WithTags("Users")
        .WithDescription("Returns a user by id")
        .RequireAuthorization("OwnData")
        .Produces<UserDTO>()
        .Produces<NotFound>(StatusCodes.Status404NotFound);
        
        group.MapPut("/{userId}/change-password", async (int userId, UpdateUserPasswordDTO updatePasswordDTO, UserManager<User> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            
            if(user == null) {
                return Results.BadRequest("Invalid user id.");
            }

            var result = await userManager.ChangePasswordAsync(
                user, 
                updatePasswordDTO.OldPassword, 
                updatePasswordDTO.NewPassword);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        })
        .WithName("ChangeUserPassword")
        .WithTags("Users")
        .WithDescription("Allows a user to change their password.")
        .RequireAuthorization("OwnData")
        .Accepts<UpdateUserPasswordDTO>("application/json")
        .Produces(StatusCodes.Status200OK)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        //[FromBody] is needed by ASP NETs .MapDelete method
        group.MapDelete("/{userId}", async ([FromBody] DeleteUserDTO deleteUserDTO, UserManager<User> userManager) =>
        {
            try {
                var user = await userManager.FindByIdAsync(deleteUserDTO.Id.ToString());

                if(user == null) {
                    return Results.BadRequest("Invalid user id.");
                }
                
                var passwordValid = await userManager.CheckPasswordAsync(user, deleteUserDTO.Password);

                if(!passwordValid) {
                    return Results.BadRequest("Invalid password");
                }
                
                await userManager.DeleteAsync(user);
                return Results.NoContent();
            }
            catch (Exception) 
            {
                //unexpected error
                return Results.Problem("An error occurred while trying to delete user.", statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .WithDescription("Deletes a user by their ID.")
        .RequireAuthorization("OwnData")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

    return group;
    }
}