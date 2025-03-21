// src\Entities\Users\DTOs\UpdateUserPasswordDTO.cs

using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Users.DTOs;

public record UpdateUserPasswordDTO(
    [Required][StringLength(100)] string oldPassword,
    [Required][StringLength(100)] string newPassword
);