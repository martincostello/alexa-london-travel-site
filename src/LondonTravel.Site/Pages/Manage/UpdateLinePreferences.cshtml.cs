// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Manage;

[Authorize]
public partial class UpdateLinePreferences(
    UserManager<LondonTravelUser> userManager,
    ITflServiceFactory tflServiceFactory,
    ILogger<UpdateLinePreferences> logger) : PageModel
{
    public async Task<IActionResult> OnPost(
        [Bind(nameof(UpdateLinePreferencesViewModel.ETag), nameof(UpdateLinePreferencesViewModel.FavoriteLines))]
        UpdateLinePreferencesViewModel model,
        CancellationToken cancellationToken)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.ETag))
        {
            return BadRequest();
        }

        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            Log.FailedToGetUserToUpdateLinePreferences(logger);
            return RedirectToPage(SiteRoutes.Error);
        }

        bool? updated = null;

        // Do not bother updating the preferences if they are they same
        bool hasModelBeenUpdated =
            model.FavoriteLines == null ||
            !model.FavoriteLines.SequenceEqual(user.FavoriteLines);

        if (hasModelBeenUpdated)
        {
            if (!await AreLinesValidAsync(model, cancellationToken))
            {
                return BadRequest();
            }

            Log.UpdatingLinePreferences(logger, user.Id);

            user.FavoriteLines = [.. (model.FavoriteLines ?? []).OrderBy((p) => p, StringComparer.Ordinal)];

            // Override the ETag with the one in the model to ensure write consistency
            user.ETag = model.ETag;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                Log.UpdatedLinePreferences(logger, user.Id);
            }
            else
            {
                Log.UpdatingLinePreferencesFailed(logger, user.Id, model.ETag);
            }

            updated = result.Succeeded;
        }

        return RedirectToPage(SiteRoutes.Home, new { UpdateSuccess = updated });
    }

    private async Task<bool> AreLinesValidAsync(UpdateLinePreferencesViewModel model, CancellationToken cancellationToken)
    {
        if (model.FavoriteLines != null)
        {
            var service = tflServiceFactory.CreateService();
            var lines = await service.GetLinesAsync(cancellationToken);

            var validLines = lines.Select((p) => p.Id).ToList();

            return model.FavoriteLines.All(validLines.Contains);
        }

        return true;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 19,
            Level = LogLevel.Error,
            Message = "Failed to get user to update line preferences.")]
        public static partial void FailedToGetUserToUpdateLinePreferences(ILogger logger);

        [LoggerMessage(
            EventId = 20,
            Level = LogLevel.Trace,
            Message = "Updating line preferences for user Id {UserId}.")]
        public static partial void UpdatingLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 21,
            Level = LogLevel.Information,
            Message = "Updated line preferences for user Id {UserId}.")]
        public static partial void UpdatedLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 22,
            Level = LogLevel.Warning,
            Message = "Failed to update line preferences for user '{UserId}' as it would cause a write conflict. ETag: {ETag}.")]
        public static partial void UpdatingLinePreferencesFailed(ILogger logger, string? userId, string etag);
    }
}
