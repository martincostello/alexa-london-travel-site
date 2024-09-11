// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MartinCostello.OpenApi;

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// Represents the API response for a user's preferences.
/// </summary>
[OpenApiExample<PreferencesResponse>]
public sealed class PreferencesResponse : IExampleProvider<PreferencesResponse>
{
    /// <summary>
    /// Gets or sets the Ids of the user's favorite lines, if any.
    /// </summary>
    [JsonPropertyName("favoriteLines")]
    [Required]
    public ICollection<string> FavoriteLines { get; set; } = [];

    /// <summary>
    /// Gets or sets the user's Id.
    /// </summary>
    [JsonPropertyName("userId")]
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <inheritdoc/>
    public static PreferencesResponse GenerateExample()
    {
        return new()
        {
            FavoriteLines = ["northern", "victoria"],
            UserId = "578a0443-2208-4fb3-8e33-92351e58b685",
        };
    }
}
