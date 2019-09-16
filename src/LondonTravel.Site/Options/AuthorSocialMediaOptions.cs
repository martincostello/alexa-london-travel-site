// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the social media options for the site author. This class cannot be inherited.
    /// </summary>
    public sealed class AuthorSocialMediaOptions
    {
        /// <summary>
        /// Gets or sets the Facebook profile Id of the author.
        /// </summary>
        public string? Facebook { get; set; }

        /// <summary>
        /// Gets or sets the Twitter handle of the author.
        /// </summary>
        public string? Twitter { get; set; }
    }
}
