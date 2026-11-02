// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class LinkAccount(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    ILogger<LinkAccount> logger) : PageModel
{
    public async Task<IActionResult> OnPost(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return BadRequest();
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        string? redirectUrl = Url.Page(SiteRoutes.LinkAccountCallback);
        string? userId = userManager.GetUserId(User);

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);

        SiteContext.SetErrorRedirect(properties, Url.Page(SiteRoutes.Manage)!);

        Log.AttemptingToLinkUser(logger, userId, provider);

        return Challenge(properties, provider);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Information,
            Message = "Attempting to link user Id {UserId} to provider {ProviderName}.")]
        public static partial void AttemptingToLinkUser(ILogger logger, string? userId, string providerName);
    }
}
