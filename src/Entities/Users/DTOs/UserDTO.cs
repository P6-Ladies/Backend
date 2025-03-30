// src\Entities\Users\DTOs\UserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Users.DTOs;
public record UserDTO
(
    [Required] int Id,
    [Required] string Email
);