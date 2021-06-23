// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;

namespace MartinCostello.LondonTravel.Site.Identity
{
    /// <summary>
    /// A class for registering external authentication events. This class cannot be inherited.
    /// </summary>
    public sealed class ExternalAuthEvents
    {
        /// <summary>
        /// Gets or sets an optional delegate to invoke when redirecting to an OAuth authorization endpoint.
        /// </summary>
        public Func<RedirectContext<OAuthOptions>, Task>? OnRedirectToOAuthAuthorizationEndpoint { get; set; }

        /// <summary>
        /// Gets or sets an optional delegate to invoke when redirecting to the Twitter authorization endpoint.
        /// </summary>
        public Func<RedirectContext<TwitterOptions>, Task>? OnRedirectToTwitterAuthorizationEndpoint { get; set; }
    }
}
