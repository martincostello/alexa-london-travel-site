// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the metadata options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class MetadataOptions
    {
        /// <summary>
        /// Gets or sets the author options.
        /// </summary>
        public AuthorOptions? Author { get; set; }

        /// <summary>
        /// Gets or sets the site description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// Gets or sets the URL of the default image.
        /// </summary>
        public string? Image { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        public string? Keywords { get; set; }

        /// <summary>
        /// Gets or sets the site name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the URL of the site's repository.
        /// </summary>
        public string? Repository { get; set; }

        /// <summary>
        /// Gets or sets the robots value.
        /// </summary>
        public string? Robots { get; set; }

        /// <summary>
        /// Gets or sets the social media options.
        /// </summary>
        public AuthorSocialMediaOptions? SocialMedia { get; set; }

        /// <summary>
        /// Gets or sets the site type.
        /// </summary>
        public string? Type { get; set; }
    }
}
