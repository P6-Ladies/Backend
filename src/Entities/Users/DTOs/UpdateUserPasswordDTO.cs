// src\Entities\Users\DTOs\UpdateUserPasswordDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Entities.Users.DTOs;
public record UpdateUserPasswordDTO
(
    [Required][StringLength(100)] string OldPassword,
    [Required][StringLength(100)] string NewPassword
);