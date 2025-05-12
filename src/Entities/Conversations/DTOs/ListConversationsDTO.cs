// src\Entities\Users\DTOs\CreateUserDTO.cs

namespace Backend.Entities.Conversations.DTOs;
public record ListConversationsDTO
{
    public List<ShowConversationDTO> Conversations { get; set; } = new List<ShowConversationDTO>(); //removed required because it was causing issues with the test

}