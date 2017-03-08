// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Extension methods to add Amazon authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class AmazonAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="AmazonMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>,
        /// which enables Amazon authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>
        /// A reference to this instance after the operation has completed.
        /// </returns>
        public static IApplicationBuilder UseAmazonAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<AmazonMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="AmazonMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>,
        /// which enables Amazon authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="AmazonOptions"/> that specifies options for the middleware.</param>
        /// <returns>
        /// A reference to this instance after the operation has completed.
        /// </returns>
        public static IApplicationBuilder UseAmazonAuthentication(this IApplicationBuilder app, AmazonOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<AmazonMiddleware>(Options.Create(options));
        }
    }
}
