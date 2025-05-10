// src\Security\Configuration\ClaimUtils.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Backend.Security.Configuration;

public static class ClaimUtils
{
    private static readonly Dictionary<string, int> RoleHierarchy = new()
    {
        { "User", 1 }
    };

    public static IdentityUserClaim<string> GetHighestClaim(List<IdentityUserClaim<string>> claims)
    {
        return claims.OrderBy(uc =>
        {
            var claimType = uc.ClaimType ?? string.Empty; 
            return RoleHierarchy.TryGetValue(claimType, out var val) ? val : 0;
        }).First();
    }
}