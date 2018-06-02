// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Net.Http;
    using Identity.Amazon;
    using MartinCostello.LondonTravel.Site.Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IdentityServiceCollectionExtensions
    {
        private const string CorrelationCookieName = "london-travel-correlation";

        private const string StateCookieName = "london-travel-state";

        /// <summary>
        /// Configures identity services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<SiteOptions>();

            if (options?.Authentication?.IsEnabled == true)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var builder = services.AddAuthentication();

                if (TryGetProvider("Amazon", options, out var amazonOptions))
                {
                    builder.AddAmazon((p) => SetupOAuth("Amazon", p, amazonOptions, loggerFactory, provider));
                }

                if (TryGetProvider("Facebook", options, out var facebookOptions))
                {
                    builder.AddFacebook((p) => SetupOAuth("Facebook", p, facebookOptions, loggerFactory, provider));
                }

                if (TryGetProvider("Google", options, out var googleOptions))
                {
                    builder.AddGoogle((p) => SetupOAuth("Google", p, googleOptions, loggerFactory, provider));
                }

                if (TryGetProvider("Microsoft", options, out var microsoftOptions))
                {
                    builder.AddMicrosoftAccount((p) => SetupOAuth("Microsoft", p, microsoftOptions, loggerFactory, provider));
                }

                if (TryGetProvider("Twitter", options, out var twitterOptions))
                {
                    builder.AddTwitter(
                        (p) =>
                        {
                            p.ConsumerKey = twitterOptions.ClientId;
                            p.ConsumerSecret = twitterOptions.ClientSecret;
                            p.RetrieveUserDetails = true;
                            p.StateCookie.Name = StateCookieName;

                            p.Events.OnRemoteFailure =
                                (context) => OAuthEventsHandler.HandleRemoteFailure(
                                    context,
                                    p.SignInScheme,
                                    p.StateDataFormat,
                                    loggerFactory.CreateLogger("Twitter"),
                                    (token) => token?.Properties?.Items);

                            SetupRemoteAuth("Twitter", p, provider);
                        });
                }
            }

            return services;
        }

        /// <summary>
        /// Sets up an instance of <typeparamref name="T"/> for an OAuth provider.
        /// </summary>
        /// <typeparam name="T">The type of the OAuth options to set up.</typeparam>
        /// <param name="name">The name of the OAuth provider to set up.</param>
        /// <param name="auth">The OAuth options to set up.</param>
        /// <param name="options">The <see cref="ExternalSignInOptions"/> to use to set up the instance.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        private static void SetupOAuth<T>(
            string name,
            T auth,
            ExternalSignInOptions options,
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider)
            where T : OAuthOptions
        {
            auth.ClientId = options.ClientId;
            auth.ClientSecret = options.ClientSecret;
            auth.Events = new OAuthEventsHandler(auth, loggerFactory);

            SetupRemoteAuth(name, auth, serviceProvider);
        }

        /// <summary>
        /// Sets up an instance of <typeparamref name="T"/> for a remote authentication provider.
        /// </summary>
        /// <typeparam name="T">The type of the remote authentication options to set up.</typeparam>
        /// <param name="name">The name of the remote authentication provider to set up.</param>
        /// <param name="options">The remote authentication options to set up.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        private static void SetupRemoteAuth<T>(string name, T options, IServiceProvider serviceProvider)
            where T : RemoteAuthenticationOptions
        {
            var factory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            options.Backchannel = factory.CreateClient(name);
            options.CorrelationCookie.Name = CorrelationCookieName;
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
