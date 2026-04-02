// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using rent_a_car.Data;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RentACarDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RentACarDbContext context,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "EGN must be 10 digits.")]
            [Display(Name = "EGN")]
            public string EGN { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EGN = user.EGN
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update phone number
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Update other user properties
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.EGN = Input.EGN;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        /// <summary>
        /// Deletes the user account and all associated reservations.
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            try
            {
                // Delete all user's reservations first
                var reservations = await _context.Reservations
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();

                _context.Reservations.RemoveRange(reservations);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} reservations for user {UserId}.", reservations.Count, user.Id);

                // Delete the user account
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to delete account.";
                    return RedirectToPage();
                }

                // Sign out the user after deletion
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User {UserId} deleted their account.", user.Id);

                return Redirect("~/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account for {UserId}.", user.Id);
                StatusMessage = "An error occurred while deleting your account.";
                return RedirectToPage();
            }
        }
    }
}