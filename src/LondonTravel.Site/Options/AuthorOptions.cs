// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the author options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class AuthorOptions
    {
        /// <summary>
        /// Gets or sets the URL of the author's avatar.
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// Gets or sets the Bitcoin address of the author.
        /// </summary>
        public string? Bitcoin { get; set; }

        /// <summary>
        /// Gets or sets the email address of the author.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the social media options.
        /// </summary>
        public AuthorSocialMediaOptions? SocialMedia { get; set; }

        /// <summary>
        /// Gets or sets the URL of the author's website.
        /// </summary>
        public string? Website { get; set; }
    }
}
