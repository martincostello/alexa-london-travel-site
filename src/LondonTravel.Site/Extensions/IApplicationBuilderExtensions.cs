// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Middleware;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref="IApplicationBuilder"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds the custom HTTP headers middleware to the pipeline.
        /// </summary>
        /// <param name="value">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="environment">The current hosting environment.</param>
        /// <param name="config">The current configuration.</param>
        /// <param name="options">The current site options.</param>
        /// <returns>
        /// The value specified by <paramref name="value"/>.
        /// </returns>
        public static IApplicationBuilder UseCustomHttpHeaders(
            this IApplicationBuilder value,
            IWebHostEnvironment environment,
            IConfiguration config,
            IOptionsSnapshot<SiteOptions> options)
        {
            return value.UseMiddleware<CustomHttpHeadersMiddleware>(environment, config, options);
        }

        /// <summary>
        /// Configures the application to use identity.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to configure.</param>
        /// <param name="options">The current site configuration.</param>
        public static void UseIdentity(this IApplicationBuilder app, SiteOptions options)
        {
            if (options?.Authentication?.IsEnabled == true)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }
    }
}
