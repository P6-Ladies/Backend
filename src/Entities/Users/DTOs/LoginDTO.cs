// src\Entities\Users\DTOs\LoginDTO.cs

namespace Backend.Entities.Users.DTOs;
public record LoginDTO
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}