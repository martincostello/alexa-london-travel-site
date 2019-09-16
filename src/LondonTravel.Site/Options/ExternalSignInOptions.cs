// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options
{
    /// <summary>
    /// A class representing the options to use as a provider for an external sign-in. This class cannot be inherited.
    /// </summary>
    public sealed class ExternalSignInOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the provider is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the client Id for the provider.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for the provider.
        /// </summary>
        public string? ClientSecret { get; set; }
    }
}
