// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class containing metadata for reviews of the skill. This class cannot be inherited.
/// </summary>
public sealed class ReviewMetadataModel
{
    /// <summary>
    /// Gets or sets the best possible score for a rating.
    /// </summary>
    public double BestRating { get; set; }

    /// <summary>
    /// Gets or sets the worst possible score for a rating.
    /// </summary>
    public double WorstRating { get; set; }

    /// <summary>
    /// Gets or sets the average rating score.
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the number of reviews.
    /// </summary>
    public int ReviewCount { get; set; }
}
