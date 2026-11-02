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
public partial class LinkAccountCallback(
    UserManager<LondonTravelUser> userManager,
    SignInManager<LondonTravelUser> signInManager,
    ILogger<LinkAccountCallback> logger) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            Log.FailedToGetUserToManageAccount(logger);
            return RedirectToPage(SiteRoutes.Error);
        }

        string? userId = await userManager.GetUserIdAsync(user);
        var info = await signInManager.GetExternalLoginInfoAsync(userId);

        if (info == null)
        {
            Log.FailedToGetExternalLogin(logger, userId);
            return RedirectToPage(SiteRoutes.Manage, new { Message = SiteMessage.Error });
        }

        Log.AddingExternalLogin(logger, info.LoginProvider, userId);

        var result = await userManager.AddLoginAsync(user, info);
        var message = SiteMessage.Error;

        if (result.Succeeded)
        {
            Log.AddedExternalLogin(logger, info.LoginProvider, userId);

            message = SiteMessage.LinkSuccess;

            result = await UpdateClaimsAsync(user, info);

            if (result.Succeeded)
            {
                Log.UpdatedUserClaims(logger, userId, info.LoginProvider);
            }
            else if (logger.IsEnabled(LogLevel.Error))
            {
#pragma warning disable CA1873
                Log.UpdatingUserClaimsFailed(
                    logger,
                    userId,
                    info.LoginProvider,
                    result.FormatErrors());
#pragma warning restore CA1873
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
        else if (logger.IsEnabled(LogLevel.Error))
        {
#pragma warning disable CA1873
            Log.AddingExternalLoginFailed(
                logger,
                userId,
                result.FormatErrors());
#pragma warning restore CA1873
        }

        return RedirectToPage(SiteRoutes.Manage, new { Message = message });
    }

    private async Task<IdentityResult> UpdateClaimsAsync(LondonTravelUser user, ExternalLoginInfo info)
    {
        bool commitUpdate = false;

        if (user.RoleClaims == null)
        {
            user.RoleClaims = [];
            commitUpdate = true;
        }

        foreach (var claim in info.Principal.Claims)
        {
            bool hasClaim = user.RoleClaims
                .Where((p) => p.ClaimType == claim.Type)
                .Where((p) => p.Issuer == claim.Issuer)
                .Where((p) => p.Value == claim.Value)
                .Any((p) => p.ValueType == claim.ValueType);

            if (!hasClaim)
            {
                user.RoleClaims.Add(LondonTravelRole.FromClaim(claim));
                commitUpdate = true;
            }
        }

        return commitUpdate
            ? await userManager.UpdateAsync(user)
            : IdentityResult.Success;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Failed to get user to manage account.")]
        public static partial void FailedToGetUserToManageAccount(ILogger logger);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Error,
            Message = "Failed to get external login info for user Id {UserId} to link account.")]
        public static partial void FailedToGetExternalLogin(ILogger logger, string userId);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Trace,
            Message = "Adding login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddingExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Information,
            Message = "Added login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddedExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Information,
            Message = "Updated claims for user Id {UserId} for provider {ProviderName}.")]
        public static partial void UpdatedUserClaims(ILogger logger, string userId, string providerName);

        [LoggerMessage(
            EventId = 7,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to update user Id {UserId} with additional role claims for provider {ProviderName}: {Errors}.")]
        public static partial void UpdatingUserClaimsFailed(ILogger logger, string userId, string providerName, string errors);

        [LoggerMessage(
            EventId = 8,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to add external login info for user Id {UserId}: {Errors}.")]
        public static partial void AddingExternalLoginFailed(ILogger logger, string userId, string errors);
    }
}
