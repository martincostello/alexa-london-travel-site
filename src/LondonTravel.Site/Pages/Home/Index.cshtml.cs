// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Home;

public partial class Index(
    UserManager<LondonTravelUser> userManager,
    ITflServiceFactory tflFactory,
    ILogger<Index> logger) : PageModel
{
    /// <summary>
    /// Gets the ETag associated with the preferences.
    /// </summary>
    public string ETag { get; private set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the user is linked to the Alexa app.
    /// </summary>
    public bool IsLinkedToAlexa { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a user has selected any favorite lines.
    /// </summary>
    public bool HasFavorites => FavoriteLines.Any((p) => p.IsFavorite);

    /// <summary>
    /// Gets the number of favorite lines the user has selected.
    /// </summary>
    public int SelectedLineCount => FavoriteLines.Count();

    /// <summary>
    /// Gets all the line(s).
    /// </summary>
    public IList<FavoriteLineItem> AllLines { get; private set; } = [];

    /// <summary>
    /// Gets the user's favorite line(s).
    /// </summary>
    public IEnumerable<FavoriteLineItem> FavoriteLines => AllLines.Where((p) => p.IsFavorite);

    /// <summary>
    /// Gets the other line(s).
    /// </summary>
    public IEnumerable<FavoriteLineItem> OtherLines => AllLines.Where((p) => !p.IsFavorite);

    /// <summary>
    /// Gets a value indicating whether the preferences were successfully updated.
    /// </summary>
    public bool? UpdateResult { get; private set; }

    public async Task OnGet(string? updateSuccess = null)
    {
        if (User.Identity?.IsAuthenticated == false)
        {
            return;
        }

        var user = await userManager.GetUserAsync(User);

        if (user == null)
        {
            Log.FailedToGetUser(logger);
            return;
        }

        ETag = user.ETag!;
        IsAuthenticated = true;
        IsLinkedToAlexa = !string.IsNullOrWhiteSpace(user.AlexaToken);

        var service = tflFactory.CreateService();
        var lines = await service.GetLinesAsync(HttpContext.RequestAborted);

        MapFavoriteLines(lines, user.FavoriteLines);

        if (!string.IsNullOrEmpty(updateSuccess))
        {
            UpdateResult = string.Equals(updateSuccess, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void OnHead()
    {
    }

    private void MapFavoriteLines(ICollection<LineInfo> tflLines, ICollection<string> userFavorites)
    {
        if (tflLines.Count == 0)
        {
            Log.FailedToMapUserPreferences(logger);
            return;
        }

        foreach (var line in tflLines.OrderBy((p) => p.Name, StringComparer.OrdinalIgnoreCase))
        {
            var favorite = new FavoriteLineItem()
            {
                DisplayName = line.Name!,
                Id = line.Id!,
                IsFavorite = userFavorites?.Contains(line.Id, StringComparer.Ordinal) is true,
            };

            AllLines.Add(favorite);
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
           EventId = 56,
           Level = LogLevel.Error,
           Message = "Failed to get user to render preferences.")]
        public static partial void FailedToGetUser(ILogger logger);

        [LoggerMessage(
           EventId = 57,
           Level = LogLevel.Error,
           Message = "Failed to map TfL lines as there were no values.")]
        public static partial void FailedToMapUserPreferences(ILogger logger);
    }
}
