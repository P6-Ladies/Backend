// src\Mappings\UserMapping.cs
using backend.Entities.Users;
using backend.Entities.Users.DTOs;

namespace backend.Mappings;

public static class UserMapping
{
    public static User ToEntity(this CreateUserDTO userDTO)
    {
        return new User
        {
            Email = userDTO.Email,
        };
    }

    public static UserDTO ToDTO(this User user)
    {
        return new UserDTO(
            user.Id!,
            user.Email!
        );
    }
}