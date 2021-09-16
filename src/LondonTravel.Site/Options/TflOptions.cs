// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options;

/// <summary>
/// A class representing the options for the TfL API integration. This class cannot be inherited.
/// </summary>
public sealed class TflOptions
{
    /// <summary>
    /// Gets or sets the app Id.
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// Gets or sets the app key.
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// Gets or sets the base URI for the TfL API.
    /// </summary>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the supported modes.
    /// </summary>
    public ICollection<string>? SupportedModes { get; set; }
}
