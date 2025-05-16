// src\Mappings\UserMapping.cs
using backend.Entities.Users;
using backend.Entities.Users.DTOs;

namespace backend.Mappings;

public static class UserMapping
{
    public static User ToEntity(this CreateUserDTO userDTO)
    {
        // Initialize a new User with Identity fields:
        return new User
        {
            Email = userDTO.Email,
            UserName = userDTO.Email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static UserDTO ToDTO(this User user)
    {
        return new UserDTO(
            user.Id,
            user.Email ?? ""
        );
    }
}
