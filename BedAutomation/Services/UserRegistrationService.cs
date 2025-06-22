using Microsoft.AspNetCore.Identity;

namespace BedAutomation.Services
{
    public class UserRegistrationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRegistrationService> _logger;

        public UserRegistrationService(
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ILogger<UserRegistrationService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task AssignDefaultRoleAsync(IdentityUser user)
        {
            try
            {
                // Ensure Patient role exists
                var patientRoleExists = await _roleManager.RoleExistsAsync("Patient");
                if (!patientRoleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Patient"));
                    _logger.LogInformation("Patient role created");
                }

                // Check if user already has any role
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Any())
                {
                    // Assign Patient role to new user
                    var result = await _userManager.AddToRoleAsync(user, "Patient");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Patient role assigned to user: {user.Email}");
                    }
                    else
                    {
                        _logger.LogError($"Failed to assign Patient role to user: {user.Email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning default role to user: {user.Email}");
            }
        }
    }
} 