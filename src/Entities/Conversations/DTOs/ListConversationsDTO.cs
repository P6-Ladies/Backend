// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Conversations.DTOs;
public record ListConversationsDTO
{
    [Required]
    public required List<ShowConversationDTO> Conversations { get; set; } = new List<ShowConversationDTO>();

}