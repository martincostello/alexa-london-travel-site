// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// A class representing the view model for page metadata. This class cannot be inherited.
/// </summary>
public sealed class MetaModel
{
    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the Bitcoin address of the author.
    /// </summary>
    public string? Bitcoin { get; set; }

    /// <summary>
    /// Gets or sets the canonical URI.
    /// </summary>
    public string? CanonicalUri { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Facebook app Id.
    /// </summary>
    public string? FacebookApp { get; set; }

    /// <summary>
    /// Gets or sets the Facebook profile Id.
    /// </summary>
    public string? FacebookProfile { get; set; }

    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the image URI.
    /// </summary>
    public string? ImageUri { get; set; }

    /// <summary>
    /// Gets or sets the image alternate text.
    /// </summary>
    public string? ImageAltText { get; set; }

    /// <summary>
    /// Gets or sets the page keywords.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets the robots value.
    /// </summary>
    public string? Robots { get; set; }

    /// <summary>
    /// Gets or sets the site name.
    /// </summary>
    public string? SiteName { get; set; }

    /// <summary>
    /// Gets or sets the site type.
    /// </summary>
    public string? SiteType { get; set; }

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the Twitter card type.
    /// </summary>
    public string? TwitterCard { get; set; }

    /// <summary>
    /// Gets or sets the Twitter handle.
    /// </summary>
    public string? TwitterHandle { get; set; }

    /// <summary>
    /// Gets or sets the reviews metadata.
    /// </summary>
    public ReviewMetadataModel? Reviews { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="MetaModel"/>.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <param name="canonicalUri">The optional canonical URI of the page.</param>
    /// <param name="hostName">The optional host name.</param>
    /// <param name="description">The optional page description.</param>
    /// <param name="imageUri">The optional image URI.</param>
    /// <param name="imageAltText">The optional image alternate text.</param>
    /// <param name="robots">The optional robots value.</param>
    /// <param name="title">The optional page title.</param>
    /// <returns>
    /// The created instance of <see cref="MetaModel"/>.
    /// </returns>
    public static MetaModel Create(
        MetadataOptions? options,
        string? canonicalUri = null,
        string? hostName = null,
        string? description = null,
        string? imageUri = null,
        string? imageAltText = null,
        string? robots = null,
        string? title = null)
    {
        options ??= new MetadataOptions();

        var model = new MetaModel();

        if (options.Author != null)
        {
            model.Author = options.Author.Name;
            model.Bitcoin = options.Author.Bitcoin;

            if (options.Author.SocialMedia != null)
            {
                model.FacebookProfile = options.Author.SocialMedia.Facebook;
                model.TwitterHandle = options.Author.SocialMedia.Twitter;
            }
        }

        model.CanonicalUri = canonicalUri ?? string.Empty;
        model.Description = description ?? options.Description;
        model.FacebookApp = options.SocialMedia?.Facebook;
        model.HostName = hostName ?? options.Domain;
        model.ImageUri = imageUri ?? options.Image ?? string.Empty;
        model.ImageAltText = imageAltText ?? options.Name;
        model.Keywords = options.Keywords ?? "alexa,london travel";
        model.Robots = robots ?? options.Robots;
        model.SiteName = options.Name ?? "London Travel is an Amazon Alexa skill for checking the status for travel in London.";
        model.SiteType = options.Type ?? "website";
        model.Title = $"{title} - {model.SiteName}";
        model.TwitterCard = "summary";

        model.Reviews = BuildReviews();

        return model;
    }

    private static ReviewMetadataModel BuildReviews()
    {
        int[] scores = [2, 3, 3, 3, 4, 4, 5, 5, 5];
        double average = Math.Round(scores.Average(), digits: 1);

        return new ReviewMetadataModel()
        {
            WorstRating = 1,
            BestRating = 5,
            ReviewCount = 7,
            AverageRating = average,
        };
    }
}
