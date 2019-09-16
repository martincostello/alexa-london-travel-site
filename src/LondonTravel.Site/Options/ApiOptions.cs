// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the API options for the site. This class cannot be inherited.
    /// </summary>
    public sealed class ApiOptions
    {
        /// <summary>
        /// Gets or sets the CORS options for the API.
        /// </summary>
        public ApiCorsOptions? Cors { get; set; }
    }
}
