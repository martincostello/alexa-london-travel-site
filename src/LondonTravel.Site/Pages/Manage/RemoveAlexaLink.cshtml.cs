// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class RemoveAlexaLink(
    UserManager<LondonTravelUser> userManager,
    ILogger<RemoveAlexaLink> logger) : PageModel
{
    public async Task<IActionResult> OnPost(string etag)
    {
        if (string.IsNullOrWhiteSpace(etag))
        {
            return BadRequest();
        }

        var user = await userManager.GetUserAsync(User);
        var message = SiteMessage.Error;

        if (user != null)
        {
            Log.RemovingAlexaLink(logger, user.Id);

            user.AlexaToken = null;
            user.ETag = etag;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                Log.RemovedAlexaLink(logger, user.Id);

                message = SiteMessage.RemoveAlexaLinkSuccess;
            }
            else if (logger.IsEnabled(LogLevel.Error))
            {
#pragma warning disable CA1873
                Log.RemovingAlexaLinkFailed(logger, user.Id, result.FormatErrors());
#pragma warning restore CA1873
            }
        }

        return RedirectToPage(SiteRoutes.Manage, new { Message = message });
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 12,
            Level = LogLevel.Trace,
            Message = "Removing Alexa link from user Id {UserId}.")]
        public static partial void RemovingAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 13,
            Level = LogLevel.Information,
            Message = "Removed Alexa link from user Id {UserId}.")]
        public static partial void RemovedAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 14,
            Level = LogLevel.Error,
            SkipEnabledCheck = true,
            Message = "Failed to remove Alexa link from user Id {UserId}: {Errors}.")]
        public static partial void RemovingAlexaLinkFailed(ILogger logger, string? userId, string errors);
    }
}
