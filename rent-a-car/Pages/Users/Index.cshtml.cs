using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Models;

namespace rent_a_car.Pages.Users
{
    /// <summary>
    /// Admin page for listing all users in the system.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public List<User> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                Users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
                _logger.LogInformation("Admin retrieved user list. Total users: {Count}", Users.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list.");
                TempData["ErrorMessage"] = "An error occurred while retrieving users.";
            }
        }

        /// <summary>
        /// Checks if a user has the Administrator role.
        /// </summary>
        public async Task<bool> IsAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, "Administrator");
        }
    }
}