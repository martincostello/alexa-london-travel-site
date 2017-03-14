// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using MartinCostello.LondonTravel.Site.Identity;
    using MartinCostello.LondonTravel.Site.Identity.Amazon;
    using MartinCostello.LondonTravel.Site.Identity.GitHub;
    using Microsoft.AspNetCore.Authentication.Twitter;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
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
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        public static void UseIdentity(this IApplicationBuilder app, SiteOptions options, ILoggerFactory loggerFactory)
        {
            if (options?.Authentication?.IsEnabled == true)
            {
                app.UseIdentity();

                if (TryGetProvider("Amazon", options, out ExternalSignInOptions provider))
                {
                    app.UseAmazonAuthentication(CreateOAuthOptions<AmazonOptions>(provider, loggerFactory));
                }

                if (TryGetProvider("Facebook", options, out provider))
                {
                    app.UseFacebookAuthentication(CreateOAuthOptions<FacebookOptions>(provider, loggerFactory));
                }

                if (TryGetProvider("Google", options, out provider))
                {
                    app.UseGoogleAuthentication(CreateOAuthOptions<GoogleOptions>(provider, loggerFactory));
                }

                if (TryGetProvider("GitHub", options, out provider))
                {
                    app.UseGitHubAuthentication(CreateOAuthOptions<GitHubOptions>(provider, loggerFactory));
                }

                if (TryGetProvider("Microsoft", options, out provider))
                {
                    app.UseMicrosoftAccountAuthentication(CreateOAuthOptions<MicrosoftAccountOptions>(provider, loggerFactory));
                }

                if (TryGetProvider("Twitter", options, out provider))
                {
                    var twitterOptions = new TwitterOptions()
                    {
                        ConsumerKey = provider.ClientId,
                        ConsumerSecret = provider.ClientSecret,
                        RetrieveUserDetails = true,
                    };

                    if (twitterOptions.Events is TwitterEvents twitterEvents)
                    {
                        twitterEvents.OnRemoteFailure =
                            (context) => OAuthEventsHandler.HandleRemoteFailure(
                                context,
                                twitterOptions.AuthenticationScheme,
                                twitterOptions.StateDataFormat,
                                loggerFactory.CreateLogger("Twitter"),
                                (token) => token?.Properties?.Items);
                    }

                    app.UseTwitterAuthentication(twitterOptions);
                }
            }
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="T"/> for an OAuth provider.
        /// </summary>
        /// <typeparam name="T">The type of the OAuth options to create.</typeparam>
        /// <param name="options">The <see cref="ExternalSignInOptions"/> to use to create the instance.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="setup">An optional delegate to use to customize the options before they are returned.</param>
        /// <returns>
        /// The created instance of <typeparamref name="T"/>.
        /// </returns>
        private static T CreateOAuthOptions<T>(ExternalSignInOptions options, ILoggerFactory loggerFactory, Action<T> setup = null)
            where T : OAuthOptions, new()
        {
            var result = new T()
            {
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
            };

            result.Events = new OAuthEventsHandler(result, loggerFactory);

            setup?.Invoke(result);

            return result;
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
