// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// A class representing information about a line. This class cannot be inherited.
/// </summary>
public sealed class LineInfo
{
    /// <summary>
    /// Gets or sets the Id of the line.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the line.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
