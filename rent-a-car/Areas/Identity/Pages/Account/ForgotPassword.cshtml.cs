// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using rent_a_car.Models;
using rent_a_car.Services;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace rent_a_car.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SmtpOptions _smtpOptions;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailSender emailSender, IOptions<SmtpOptions> smtpOptions)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _smtpOptions = smtpOptions?.Value ?? new SmtpOptions();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        // Expose a friendly message to the page (optional)
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // If SMTP is not configured, show helpful message instead of silently failing.
            if (string.IsNullOrWhiteSpace(_smtpOptions.Host)
                || string.IsNullOrWhiteSpace(_smtpOptions.SenderEmail)
                || string.IsNullOrWhiteSpace(_smtpOptions.Username)
                || string.IsNullOrWhiteSpace(_smtpOptions.Password))
            {
                ModelState.AddModelError(string.Empty, "Email sending is not configured for this site. Contact the administrator or set SMTP credentials to enable password reset emails.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Inform the user the email wasn't found.
                // NOTE: revealing account existence is a security decision — you requested explicit feedback.
                ModelState.AddModelError(string.Empty, "No account with that email address was found.");
                return Page();
            }

            // Generate reset token and send the email as before
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            StatusMessage = "If the email exists, a password reset link has been sent. Check your inbox.";
            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
