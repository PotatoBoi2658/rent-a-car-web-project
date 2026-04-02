using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Users
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<EditModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public User User { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            public string Id { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be 10 digits.")]
            [Display(Name = "EGN")]
            public string EGN { get; set; }

            [Display(Name = "Is Administrator")]
            public bool IsAdmin { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit page accessed with empty ID.");
                return NotFound();
            }

            User = await _userManager.FindByIdAsync(id);
            if (User == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for editing.", id);
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(User, "Administrator");

            Input = new InputModel
            {
                Id = User.Id,
                FirstName = User.FirstName,
                LastName = User.LastName,
                Username = User.UserName,
                Email = User.Email,
                PhoneNumber = User.PhoneNumber,
                EGN = User.EGN,
                IsAdmin = isAdmin
            };

            _logger.LogInformation("User {UserId} loaded for editing.", id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            // Log ALL model state errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning("Model validation error: {ErrorMessage}", error.ErrorMessage ?? error.Exception?.Message);
                }
            }

            _logger.LogInformation("Edit POST received. Route ID: {RouteId}, Input.Id: {InputId}, ModelState Valid: {IsValid}", 
                id, Input?.Id, ModelState.IsValid);

            if (!ModelState.IsValid)
                return Page();

            var userId = id ?? Input?.Id;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("No user ID provided in either route or form.");
                return NotFound();
            }

            try
            {
                User = await _userManager.FindByIdAsync(userId);
                if (User == null)
                {
                    _logger.LogError("User with ID {UserId} not found during POST.", userId);
                    return NotFound();
                }

                _logger.LogInformation("User {UserId} found. Updating properties.", userId);

                User.FirstName = Input.FirstName;
                User.LastName = Input.LastName;
                User.Email = Input.Email;
                User.NormalizedEmail = Input.Email?.ToUpper();
                User.PhoneNumber = Input.PhoneNumber;
                User.EGN = Input.EGN;

                var result = await _userManager.UpdateAsync(User);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user {UserId}. Errors: {@Errors}", userId, result.Errors);
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                _logger.LogInformation("User {UserId} properties updated successfully.", userId);

                // Handle role assignment - remove all roles and assign the correct one
                var currentRoles = await _userManager.GetRolesAsync(User);
                
                // Remove all existing roles
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(User, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        _logger.LogError("Failed to remove roles from user {UserId}.", userId);
                        foreach (var error in removeResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                }

                // Assign the correct role based on IsAdmin checkbox
                var roleToAssign = Input.IsAdmin ? "Administrator" : "User";
                var addRoleResult = await _userManager.AddToRoleAsync(User, roleToAssign);
                
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign {Role} role to user {UserId}.", roleToAssign, userId);
                    foreach (var error in addRoleResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                _logger.LogInformation("User {UserId} assigned {Role} role.", userId, roleToAssign);

                StatusMessage = $"✓ User '{User.UserName}' has been updated successfully!";
                _logger.LogInformation("User {UserId} updated successfully.", userId);
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating user {UserId}.", userId);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user. Please try again.");
                return Page();
            }
        }
    }
}