using Microsoft.AspNetCore.Identity;
using BedAutomation.Services;

namespace BedAutomation.Middleware
{
    public class AutoRoleAssignmentMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoRoleAssignmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, 
            UserManager<IdentityUser> userManager, 
            UserRegistrationService userRegistrationService)
        {
            // Check if user just logged in and doesn't have any roles
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                    if (!userRoles.Any())
                    {
                        // User has no roles, assign default Patient role
                        await userRegistrationService.AssignDefaultRoleAsync(user);
                    }
                }
            }

            await _next(context);
        }
    }
} 