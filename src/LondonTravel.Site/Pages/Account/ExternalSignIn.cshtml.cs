// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public class ExternalSignIn(
    SignInManager<LondonTravelUser> signInManager,
    SiteOptions siteOptions) : PageModel
{
    public IActionResult OnPost(string? provider, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest();
        }

        if (!AccountHelpers.IsSignInEnabled(siteOptions))
        {
            return NotFound();
        }

        string? redirectUrl = Url.Page(SiteRoutes.ExternalSignInCallback, new { ReturnUrl = returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        string errorRedirectUrl = GetErrorRedirectUrl();
        SiteContext.SetErrorRedirect(properties, errorRedirectUrl);

        return Challenge(properties, provider);
    }

    private string GetErrorRedirectUrl()
    {
        string? registerUrl = Url.Page(SiteRoutes.Register);

        return (IsReferrerRegistrationPage(registerUrl) ? registerUrl : Url.Page(SiteRoutes.SignIn))!;
    }

    private bool IsReferrerRegistrationPage(string? registerUrl)
        => AccountHelpers.IsPath(Request.Headers.Referer, registerUrl);
}
