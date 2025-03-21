// src\Security\Configuration\ClaimUtils.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace backend.Security.Configuration;

public static class ClaimUtils
{
    private static readonly Dictionary<string, int> RoleHierarchy = new()
    {
        { "User", 1 }
    };

    public static IdentityUserClaim<string> GetHighestClaim(List<IdentityUserClaim<string>> claims)
    {
        return claims.OrderBy(uc => RoleHierarchy.TryGetValue(uc.ClaimType, out var value) ? value : 0).First();
    }
}