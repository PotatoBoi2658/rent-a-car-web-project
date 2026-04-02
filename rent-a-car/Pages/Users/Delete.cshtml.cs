using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using rent_a_car.Models;

namespace rent_a_car.Pages.Users
{
    /// <summary>
    /// Admin page for deleting users from the system.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RentACarDbContext _context;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            UserManager<User> userManager,
            RentACarDbContext context,
            ILogger<DeleteModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public User User { get; set; }
        public bool IsAdmin { get; set; }
        public int ReservationCount { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            User = await _userManager.FindByIdAsync(id);
            if (User == null)
                return NotFound();

            IsAdmin = await _userManager.IsInRoleAsync(User, "Administrator");

            // Count user's reservations
            ReservationCount = await _context.Reservations
                .Where(r => r.UserId == id)
                .CountAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            User = await _userManager.FindByIdAsync(id);
            if (User == null)
                return NotFound();

            try
            {
                // Delete all user's reservations first (cascade delete)
                var reservations = await _context.Reservations
                    .Where(r => r.UserId == id)
                    .ToListAsync();

                _context.Reservations.RemoveRange(reservations);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} reservations for user {UserId}.", reservations.Count, id);

                // Delete the user
                var result = await _userManager.DeleteAsync(User);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                _logger.LogInformation("User {UserId} ({UserName}) deleted successfully.", User.Id, User.UserName);
                TempData["SuccessMessage"] = $"✓ User '{User.UserName}' has been deleted!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the user.");
                return Page();
            }
        }
    }
}