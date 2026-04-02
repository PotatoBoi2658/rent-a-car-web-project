using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Pages.Users
{
    /// <summary>
    /// Admin page for creating new users in the system.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class CreateModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<CreateModel> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
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

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Is Administrator")]
            public bool IsAdmin { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                // Check for existing username
                var existingUser = await _userManager.FindByNameAsync(Input.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Input.Username", "Username is already in use.");
                    return Page();
                }

                // Check for existing email
                var existingEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "Email is already in use.");
                    return Page();
                }

                // Check for existing EGN
                var existingEGN = await _userManager.Users.AnyAsync(u => u.EGN == Input.EGN);
                if (existingEGN)
                {
                    ModelState.AddModelError("Input.EGN", "EGN is already in use.");
                    return Page();
                }

                // Check for existing phone number (if provided)
                if (!string.IsNullOrWhiteSpace(Input.PhoneNumber))
                {
                    var existingPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == Input.PhoneNumber);
                    if (existingPhone)
                    {
                        ModelState.AddModelError("Input.PhoneNumber", "Phone number is already in use.");
                        return Page();
                    }
                }

                // If any validation failed, redisplay form
                if (!ModelState.IsValid)
                    return Page();

                // Create new user
                var user = new User
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    PhoneNumber = Input.PhoneNumber,
                    EGN = Input.EGN
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }

                // Assign role
                if (Input.IsAdmin)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Administrator");
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                    _logger.LogInformation("User {UserId} created as Administrator.", user.Id);
                }
                else
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                    _logger.LogInformation("User {UserId} created as regular User.", user.Id);
                }

                _logger.LogInformation("New user created: {UserId} ({Email})", user.Id, user.Email);
                TempData["SuccessMessage"] = $"✓ User '{user.UserName}' has been created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new user.");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user.");
                return Page();
            }
        }
    }
}