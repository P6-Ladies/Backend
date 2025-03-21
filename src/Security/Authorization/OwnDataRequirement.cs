// src\Security\Authorization\OwnDataRequirement.cs

using backend.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace backend.Security.Authorization;

public class OwnDataRequirement : IAuthorizationRequirement;

public class OwnDataAuthorizationHandler : AuthorizationHandler<OwnDataRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    
    public OwnDataAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor, 
        UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }
    
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnDataRequirement requirement)
    {
        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            context.Fail();
            return;
        }
        
        var httpContext = _httpContextAccessor.HttpContext;
        var userIdInUrl = httpContext.Request.RouteValues["userId"].ToString();
        
        var targetUser = await _userManager.FindByIdAsync(userIdInUrl);
        if (targetUser == null)
        {
            context.Succeed(requirement);
            return;
        }

        if (userId == userIdInUrl)
        {
            context.Succeed(requirement);
            return;
        }
        
        context.Fail();
    }
}