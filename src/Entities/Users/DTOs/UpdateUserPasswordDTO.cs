// src\Entities\Users\DTOs\UpdateUserPasswordDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Users.DTOs;
public record UpdateUserPasswordDTO
(
    [Required][StringLength(100)] string OldPassword,
    [Required][StringLength(100)] string NewPassword
);
public class ForgotPasswordDTO
{
    public required string Email { get; set; }
}

public class ResetPasswordDTO
{
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
}
