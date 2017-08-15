// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Extension methods to add Amazon authentication.
    /// </summary>
    public static class AmazonExtensions
    {
        public static AuthenticationBuilder AddAmazon(this AuthenticationBuilder builder, Action<AmazonOptions> configureOptions)
            => builder.AddAmazon(AmazonDefaults.AuthenticationScheme, configureOptions);

        public static AuthenticationBuilder AddAmazon(this AuthenticationBuilder builder, string authenticationScheme, Action<AmazonOptions> configureOptions)
            => builder.AddAmazon(authenticationScheme, AmazonDefaults.DisplayName, configureOptions);

        public static AuthenticationBuilder AddAmazon(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<AmazonOptions> configureOptions)
            => builder.AddOAuth<AmazonOptions, AmazonHandler>(authenticationScheme, displayName, configureOptions);
    }
}
