// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    using System;

    /// <summary>
    /// A class representing the external link options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class ExternalLinksOptions
    {
        /// <summary>
        /// Gets or sets the URI of the API.
        /// </summary>
        public Uri? Api { get; set; }

        /// <summary>
        /// Gets or sets the URI of the CDN.
        /// </summary>
        public Uri? Cdn { get; set; }

        /// <summary>
        /// Gets or sets the URI of the skill.
        /// </summary>
        public Uri? Skill { get; set; }

        /// <summary>
        /// Gets or sets the options for the URIs to use for reports.
        /// </summary>
        public ReportOptions? Reports { get; set; }
    }
}
