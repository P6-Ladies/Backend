// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Messages.DTOs;
public record MessageDTO
{
    [Required]
    [StringLength(2400)]
    public required string Body { get; set; }

    [Required]
    public required bool UserSent { get; set; }

    [Required]
    public required DateTime ReceivedAt { get; set; }

}