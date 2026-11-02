// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class DeleteAccount(
    UserManager<LondonTravelUser> userManager,
    ILogger<DeleteAccount> logger) : PageModel
{
    public async Task<IActionResult> OnPost()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is not null)
        {
            Log.DeletingUser(logger, user.Id);

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                Log.DeletedUser(logger, user.Id);

                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                return RedirectToPage(SiteRoutes.Home, new { Message = SiteMessage.AccountDeleted });
            }
            else if (logger.IsEnabled(LogLevel.Error))
            {
#pragma warning disable CA1873
                Log.DeletingUserFailed(logger, user.Id, result.FormatErrors());
#pragma warning restore CA1873
            }
        }
        else
        {
            Log.FailedToGetUserToDeleteAccount(logger);
        }

        return RedirectToPage(SiteRoutes.Manage, new { Message = SiteMessage.Error });
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 15,
            Level = LogLevel.Trace,
            Message = "Deleting user Id {UserId}.")]
        public static partial void DeletingUser(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 16,
            Level = LogLevel.Information,
            Message = "Deleted user Id {UserId}.")]
        public static partial void DeletedUser(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 17,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to delete user Id {UserId}: {Errors}.")]
        public static partial void DeletingUserFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
            EventId = 18,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to get user to delete account.")]
        public static partial void FailedToGetUserToDeleteAccount(ILogger logger);
    }
}
