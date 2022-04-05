using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model;
using Microsoft.AspNetCore.Authorization;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Server.Configuration;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Pages.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ApplicationSettings = Amusoft.PCR.Server.Configuration.ApplicationSettings;

namespace Amusoft.PCR.Server.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new ();

        [BindProperty(SupportsGet = true)]
        public string SelectedEmail { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public List<string> IdleUserNames { get; set; } = new();
        
        public async Task<IActionResult> OnGetAsync()
        {
            var settings = HttpContext.RequestServices.GetRequiredService<IOptionsSnapshot<ApplicationSettings>>();
            var threshold = DateTime.Now - settings.Value.Authentication.IdleResetThreshold;
            var names = await _userManager.Users.Where(d => d.LastSignIn == null || d.LastSignIn.Value < threshold)
                .Select(d => d.UserName)
                .ToArrayAsync();
            
            IdleUserNames.Clear();
            IdleUserNames.AddRange(names);

            if (!string.IsNullOrEmpty(SelectedEmail))
                Input.Email = SelectedEmail;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
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

                var channel = HttpContext.RequestServices.GetRequiredService<IUserContextChannel>();
                var request = new ChangeUserPasswordRequest();
                request.UserName = Input.Email;
                var response = await channel.SetUserPassword(request);
                if (!response.Success)
                {
                    ModelState.AddModelError("PasswordChange", "Failed to set new password");
                    return Page();
                }

                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, response.Content);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
