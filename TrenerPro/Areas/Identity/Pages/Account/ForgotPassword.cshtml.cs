// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace TrenerPro.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null )
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

                string emailTemplate = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px; background-color: #1a1a1a; color: #ffffff;'>
    <div style='text-align: center; margin-bottom: 20px;'>
        <h2 style='color: #8b5cf6;'>💪 BeFit - System Trenera</h2>
    </div>
    
    <div style='background-color: #2d2d2d; padding: 20px; border-radius: 8px; text-align: center;'>
        <h3 style='margin-top: 0; color: #ffffff;'>Resetowanie hasła</h3>
        <p style='color: #cccccc; line-height: 1.5; font-size: 16px;'>
            Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta. Jeśli to nie Ty, zignoruj tę wiadomość.
        </p>
        <p style='color: #cccccc; line-height: 1.5;'>Aby ustawić nowe hasło, kliknij poniższy przycisk:</p>
        
        <div style='margin: 30px 0;'>
            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='background-color: #8b5cf6; color: white; padding: 14px 28px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block; font-size: 16px;'>Zresetuj hasło</a>
        </div>

        <p style='color: #888888; font-size: 12px; border-top: 1px solid #444; padding-top: 15px; text-align: left;'>
            Jeśli przycisk nie działa, skopiuj i wklej ten link bezpośrednio do przeglądarki:<br>
            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #8b5cf6; word-break: break-all;'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
        </p>
    </div>
</div>";

                // Wysyłamy maila z użyciem szablonu
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "BeFit - Zresetuj swoje hasło",
                    emailTemplate);
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
