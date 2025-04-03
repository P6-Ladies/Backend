// src\Entities\Users\DTOs\DeleteUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Entities.Users.DTOs;
public record DeleteUserDTO
{
    [Required] public required int Id { get; set; }

    [Required] public required string Password { get; set; }
}