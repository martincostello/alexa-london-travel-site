// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class representing the view model to update a user's line preferences.
/// </summary>
public class UpdateLinePreferencesViewModel
{
    /// <summary>
    /// Gets or sets the ETag value associated with the user's preferences.
    /// </summary>
    public string ETag { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of the user's favorite line(s).
    /// </summary>
    public IList<string> FavoriteLines { get; set; } = [];
}
