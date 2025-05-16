// src\Security\Authorization\OwnDataRequirement.cs

using backend.Entities.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace backend.Security.Authorization;

public class OwnDataRequirement : IAuthorizationRequirement;

public class OwnDataAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor,
    UserManager<User> userManager) : AuthorizationHandler<OwnDataRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserManager<User> _userManager = userManager;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnDataRequirement requirement)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            context.Fail();
            return;
        }

        var userId = _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        if (userId == null)
        {
            context.Fail();
            return;
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            context.Fail();
            return;
        }
        
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return;
        }
        var routeValue = httpContext.Request.RouteValues["userId"];
        var userIdInUrl = routeValue?.ToString();

        if (string.IsNullOrEmpty(userIdInUrl))
        {
            context.Fail();
            return;
        }
        
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