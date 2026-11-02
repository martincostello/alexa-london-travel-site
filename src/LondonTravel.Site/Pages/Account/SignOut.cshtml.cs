// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

[Authorize]
public partial class SignOut(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    SiteOptions siteOptions,
    ILogger<SignOut> logger) : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        if (!AccountHelpers.IsSignInEnabled(siteOptions))
        {
            return NotFound();
        }

        string? userId = userManager.GetUserId(User);

        await signInManager.SignOutAsync();

        Log.UserSignedOut(logger, userId);

        return RedirectToPage(SiteRoutes.Home);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "User Id {UserId} signed out.")]
        public static partial void UserSignedOut(ILogger logger, string? userId);
    }
}
