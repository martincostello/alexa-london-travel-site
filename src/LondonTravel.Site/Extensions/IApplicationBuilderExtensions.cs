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
            IHostingEnvironment environment,
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
                app.UseIdentity();

                ExternalSignInOptions provider;

                if (TryGetProvider("Facebook", options, out provider))
                {
                    app.UseFacebookAuthentication(new FacebookOptions() { ClientId = provider.ClientId, ClientSecret = provider.ClientSecret });
                }

                if (TryGetProvider("Google", options, out provider))
                {
                    app.UseGoogleAuthentication(new GoogleOptions() { ClientId = provider.ClientId, ClientSecret = provider.ClientSecret });
                }

                if (TryGetProvider("Microsoft", options, out provider))
                {
                    app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions() { ClientId = provider.ClientId, ClientSecret = provider.ClientSecret });
                }

                if (TryGetProvider("Twitter", options, out provider))
                {
                    app.UseTwitterAuthentication(new TwitterOptions() { ConsumerKey = provider.ClientId, ConsumerSecret = provider.ClientSecret, RetrieveUserDetails = true });
                }
            }
        }

        /// <summary>
        /// Tries to get the external sign-in settings for the specified provider.
        /// </summary>
        /// <param name="name">The name of the provider to get the provider settings for.</param>
        /// <param name="options">The current site options.</param>
        /// <param name="provider">When the method returns, containsint the provider settings, if enabled.</param>
        /// <returns>
        /// <see langword="true"/> if the specified provider is enabled; otherwise <see langword="false"/>.
        /// </returns>
        private static bool TryGetProvider(string name, SiteOptions options, out ExternalSignInOptions provider)
        {
            provider = null;
            ExternalSignInOptions signInOptions = null;

            bool isEnabled =
                options?.Authentication?.ExternalProviders?.TryGetValue(name, out signInOptions) == true &&
                signInOptions?.IsEnabled == true &&
                !string.IsNullOrEmpty(signInOptions?.ClientId) &&
                !string.IsNullOrEmpty(signInOptions?.ClientSecret);

            if (isEnabled)
            {
                provider = signInOptions;
            }

            return isEnabled;
        }
    }
}
