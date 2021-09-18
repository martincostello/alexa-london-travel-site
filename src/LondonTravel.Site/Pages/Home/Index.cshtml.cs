// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Home;

public class Index : PageModel
{
    private readonly UserManager<LondonTravelUser> _userManager;
    private readonly ITflServiceFactory _tflFactory;
    private readonly ILogger _logger;

    public Index(
        UserManager<LondonTravelUser> userManager,
        ITflServiceFactory tflFactory,
        ILogger<Index> logger)
    {
        _userManager = userManager;
        _tflFactory = tflFactory;
        _logger = logger;
    }

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
    public IList<FavoriteLineItem> AllLines { get; private set; } = new List<FavoriteLineItem>();

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

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            Log.FailedToGetUser(_logger);
            return;
        }

        ETag = user.ETag!;
        IsAuthenticated = true;
        IsLinkedToAlexa = !string.IsNullOrWhiteSpace(user.AlexaToken);

        ITflService service = _tflFactory.CreateService();
        ICollection<LineInfo> lines = await service.GetLinesAsync(HttpContext.RequestAborted);

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
            Log.FailedToMapUserPreferences(_logger);
            return;
        }

        foreach (LineInfo line in tflLines)
        {
            var favorite = new FavoriteLineItem()
            {
                DisplayName = line.Name!,
                Id = line.Id!,
            };

            AllLines.Add(favorite);
        }

        AllLines = AllLines
            .OrderBy((p) => p.DisplayName, StringComparer.Ordinal)
            .ToList();

        if (userFavorites?.Count > 0)
        {
            foreach (var favorite in AllLines)
            {
                favorite.IsFavorite = userFavorites.Contains(favorite.Id, StringComparer.Ordinal);
            }
        }
    }
}
