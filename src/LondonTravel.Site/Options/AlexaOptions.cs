// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the Alexa options for the site. This class cannot be inherited.
    /// </summary>
    public class AlexaOptions
    {
        /// <summary>
        /// Gets or sets the client Id for the Alexa skill.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Alexa account linking is enabled.
        /// </summary>
        public bool IsLinkingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the allowed redirection URLs for account linking.
        /// </summary>
        public ICollection<string>? RedirectUrls { get; set; }
    }
}
