// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Users.DTOs;
public record CreateUserDTO
{
    [Required]
    [StringLength(50)]
    public required string Email { get; set; }

    [Required]
    [StringLength(100)]
    public required string Password { get; set; }
}