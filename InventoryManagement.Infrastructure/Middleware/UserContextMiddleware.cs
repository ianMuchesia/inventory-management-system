






using System.Security.Claims;
using InventoryManagement.Application.Persistence.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Infrastructure.Middleware
{
    public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    
    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context, IUserRepository userService)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userService.GetByIdAsync(int.Parse(userId??string.Empty));
            if(user != null)
            {
                return;
            }
            context.Items["CurrentUser"] = user;
        }
        
        await _next(context);
    }
}
}