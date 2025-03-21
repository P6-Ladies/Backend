// src\Entities\Users\User.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace backend.Entities.Users;

public class User : IdentityUser
{
    [StringLength(20)]
    public override string? Email 
    { 
        get => base.Email; 
        set => base.Email = value; 
    }
    
    public override string? NormalizedEmail 
    { 
        get => base.NormalizedEmail; 
        set => base.NormalizedEmail = value; 
    }
}