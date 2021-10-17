// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Options;

/// <summary>
/// A class representing the authentication options for the site. This class cannot be inherited.
/// </summary>
public sealed class AuthenticationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether authentication with the website is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the external sign-in providers for the site.
    /// </summary>
    public IDictionary<string, ExternalSignInOptions>? ExternalProviders { get; set; }

    /// <summary>
    /// Gets or sets the user store options.
    /// </summary>
    public UserStoreOptions? UserStore { get; set; }
}
