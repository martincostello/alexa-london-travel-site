// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class representing the view model for the help page.
/// </summary>
public sealed class HelpViewModel
{
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
}
