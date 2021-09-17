// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MartinCostello.LondonTravel.Site.Pages.Help;

public class Index : PageModel
{
    private readonly UserManager<LondonTravelUser> _userManager;

    public Index(UserManager<LondonTravelUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user has at least one favorite line.
    /// </summary>
    public bool HasFavorites { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user has linked their account to Alexa.
    /// </summary>
    public bool IsLinkedToAlexa { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is signed in.
    /// </summary>
    public bool IsSignedIn { get; set; }

    /// <summary>
    /// Gets a value indicating whether to show the account linking note.
    /// </summary>
    public bool ShowLinkingNote => !IsSignedIn || !IsLinkedToAlexa || !HasFavorites;

    public async Task OnGet()
    {
        IsSignedIn = User?.Identity?.IsAuthenticated == true;

        if (IsSignedIn)
        {
            var user = await _userManager.GetUserAsync(User);

            HasFavorites = user?.FavoriteLines?.Count > 0;
            IsLinkedToAlexa = !string.IsNullOrEmpty(user?.AlexaToken);
        }
    }
}
