// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using rent_a_car.Models;
using System.ComponentModel.DataAnnotations;

namespace rent_a_car.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Email or username")]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var identifier = Input.Email?.Trim();
                if (string.IsNullOrWhiteSpace(identifier))
                {
                    ModelState.AddModelError(string.Empty, "Email or username is required.");
                    return Page();
                }

                // Try to find the user by username first, then by email.
                var user = await _userManager.FindByNameAsync(identifier);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(identifier);
                }

                if (user == null)
                {
                    _logger.LogWarning("Invalid login attempt: no user found for identifier: {Identifier}", identifier);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                // Verify the password (this does not sign the user in)
                var checkResult = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);

                if (checkResult.Succeeded)
                {
                    // Sign the user in
                    await _signInManager.SignInAsync(user, Input.RememberMe);
                    _logger.LogInformation("User logged in successfully with identifier: {Identifier}", identifier);
                    return LocalRedirect(returnUrl);
                }

                if (checkResult.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out for identifier: {Identifier}", identifier);
                    return RedirectToPage("./Lockout");
                }

                if (checkResult.RequiresTwoFactor)
                {
                    // Redirect to 2FA page if configured
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (checkResult.IsNotAllowed)
                {
                    _logger.LogWarning("Login not allowed for identifier: {Identifier}", identifier);
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for identifier: {Identifier}", identifier);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}