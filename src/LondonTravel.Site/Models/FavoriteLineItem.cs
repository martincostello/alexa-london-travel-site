// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class representing a favorite line.
/// </summary>
public class FavoriteLineItem
{
    /// <summary>
    /// Gets or sets the Id of the line.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the line.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the line is a favorite.
    /// </summary>
    public bool IsFavorite { get; set; }
}
