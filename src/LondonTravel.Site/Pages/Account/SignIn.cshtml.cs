// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class SignIn(SiteOptions siteOptions) : PageModel
{
    /// <summary>
    /// Gets a value indicating whether the user is signing in to link their account to the Alexa skill.
    /// </summary>
    public bool IsAlexaAuthorization { get; private set; }

    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        if (!AccountHelpers.IsSignInEnabled(siteOptions))
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage(SiteRoutes.Home);
        }

        Uri? returnUri = null;

        if (returnUrl != null &&
            !Uri.TryCreate(returnUrl, UriKind.Relative, out returnUri))
        {
            return RedirectToPage(SiteRoutes.Home);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;

        if (AccountHelpers.IsAlexaAuthorization(returnUri?.ToString()))
        {
            IsAlexaAuthorization = true;

            string[] authenticationSchemesDisabledForAlexa = ["apple", "github", "google"];
            ViewData["AuthenticationSchemesToHide"] = authenticationSchemesDisabledForAlexa;
        }

        return Page();
    }
}
