// src\Endpoints\UserEndpoints.cs

using backend.Entities.Users;
using backend.Entities.Users.DTOs;
using backend.Mappings;
using FluentEmail.Core;
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
        group.MapPost("/", async (
            CreateUserDTO newUser, 
            UserManager<User> userManager) =>
        {
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
        group.MapGet("/{userId}", async (
            int userId, 
            UserManager<User> userManager) =>
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
        
        group.MapPut("/{userId}/change-password", async (
            int userId, 
            UpdateUserPasswordDTO updatePasswordDTO, 
            UserManager<User> userManager) =>
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

        group.MapPost("/forgot-password", async (
            ForgotPasswordDTO forgotPasswordDTO,
            UserManager<User> userManager,
            IFluentEmail mailer) =>
        {
            var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);
            if (user == null)
            {
                return Results.Ok("If that email was valid, a reset email has been sent.");
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            var resetUrl = $"https://localhost/reset-password?email={forgotPasswordDTO.Email}&token={Uri.EscapeDataString(resetToken)}";//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            await mailer
                .To(forgotPasswordDTO.Email)
                .Subject("Password Reset Instructions")
                .Body($@"Click here to reset your password:
                        {resetUrl}

                        If you didnt request a reset, you can ignore this email.")
                .SendAsync();

            return Results.Ok("If that email was valid, a reset email has been sent.");
        })
        .WithName("ForgotPassword")
        .WithTags("Users")
        .WithDescription("Generates a password reset token and sends it to the user via email.")
        .Produces(StatusCodes.Status200OK);

        group.MapPut("/reset-password", async (
            ResetPasswordDTO resetPasswordDTO,
            UserManager<User> userManager) =>
        {
            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user == null)
            {
                return Results.BadRequest("Invalid user or email address.");
            }

            var result = await userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);
            if (!result.Succeeded)
            {
                return Results.BadRequest(result.Errors);
            }

            return Results.Ok("Password reset successful.");
        })
        .WithName("ResetPassword")
        .WithTags("Users")
        .WithDescription("Resets a users password using the provided token.")
        .Accepts<ResetPasswordDTO>("application/json")
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