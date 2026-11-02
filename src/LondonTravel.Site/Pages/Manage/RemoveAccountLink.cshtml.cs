// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class RemoveAccountLink(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    ILogger<RemoveAccountLink> logger) : PageModel
{
    public async Task<IActionResult> OnPost(RemoveExternalService account)
    {
        if (account == null ||
            string.IsNullOrWhiteSpace(account.LoginProvider) ||
            string.IsNullOrWhiteSpace(account.ProviderKey))
        {
            return BadRequest();
        }

        var user = await userManager.GetUserAsync(User);
        var message = SiteMessage.Error;

        if (user != null)
        {
            Log.RemovingExternalLogin(logger, account.LoginProvider, user.Id);

            var result = await userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);

            if (result.Succeeded)
            {
                Log.RemovedExternalLogin(logger, account.LoginProvider, user.Id);

                await signInManager.SignInAsync(user, isPersistent: true);

                message = SiteMessage.RemoveAccountLinkSuccess;
            }
            else if (logger.IsEnabled(LogLevel.Error))
            {
#pragma warning disable CA1873
                Log.RemovingExternalLoginFailed(
                    logger,
                    user.Id,
                    account.LoginProvider,
                    result.FormatErrors());
#pragma warning restore CA1873
            }
        }

        return RedirectToPage(SiteRoutes.Manage, new { Message = message });
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 9,
            Level = LogLevel.Trace,
            Message = "Removing login for provider {ProviderName} from user Id {UserId}.")]
        public static partial void RemovingExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
            EventId = 10,
            Level = LogLevel.Information,
            Message = "Removed login for {ProviderName} from user Id {UserId}.")]
        public static partial void RemovedExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
            EventId = 11,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to remove external login info from user Id {UserId} for provider {ProviderName}: {Errors}.")]
        public static partial void RemovingExternalLoginFailed(ILogger logger, string? userId, string providerName, string errors);
    }
}
