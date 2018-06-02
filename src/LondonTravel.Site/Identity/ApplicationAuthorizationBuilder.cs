// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Net.Http;
    using MartinCostello.LondonTravel.Site.Identity.Amazon;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class representing a builder to use to configure authorization for the application. This class cannot be inherited.
    /// </summary>
    public sealed class ApplicationAuthorizationBuilder
    {
        private readonly AuthenticationBuilder _builder;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly SiteOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationAuthorizationBuilder"/> class.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to use.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        public ApplicationAuthorizationBuilder(AuthenticationBuilder builder, IServiceProvider serviceProvider)
        {
            _builder = builder;
            _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            _options = serviceProvider.GetRequiredService<SiteOptions>();
        }

        /// <summary>
        /// Tries to configure Amazon authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddAmazon()
        {
            string name = "Amazon";

            if (TryGetProvider(name, out ExternalSignInOptions signInOptions))
            {
                _builder.AddAmazon((auth) => ConfigureOAuth(name, auth, signInOptions));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure Facebook authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddFacebook()
        {
            string name = "Facebook";

            if (TryGetProvider(name, out ExternalSignInOptions signInOptions))
            {
                _builder.AddFacebook((auth) => ConfigureOAuth(name, auth, signInOptions));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure Google authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddGoogle()
        {
            string name = "Google";

            if (TryGetProvider(name, out ExternalSignInOptions signInOptions))
            {
                _builder.AddGoogle((auth) => ConfigureOAuth(name, auth, signInOptions));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure Microsoft authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddMicrosoft()
        {
            string name = "Microsoft";

            if (TryGetProvider(name, out ExternalSignInOptions signInOptions))
            {
                _builder.AddMicrosoftAccount((auth) => ConfigureOAuth(name, auth, signInOptions));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure Twitter authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddTwitter()
        {
            string name = "Twitter";

            if (TryGetProvider(name, out ExternalSignInOptions signInOptions))
            {
                _builder.AddTwitter(
                    (options) =>
                    {
                        options.ConsumerKey = signInOptions.ClientId;
                        options.ConsumerSecret = signInOptions.ClientSecret;
                        options.RetrieveUserDetails = true;
                        options.StateCookie.Name = ApplicationCookie.State.Name;

                        options.Events.OnRemoteFailure =
                            (context) => OAuthEventsHandler.HandleRemoteFailure(
                                context,
                                options.SignInScheme,
                                options.StateDataFormat,
                                _loggerFactory.CreateLogger(name),
                                (token) => token?.Properties?.Items);

                        ConfigureRemoteAuth(name, options);
                    });
            }

            return this;
        }

        /// <summary>
        /// Configures an instance of <typeparamref name="T"/> for an OAuth provider.
        /// </summary>
        /// <typeparam name="T">The type of the OAuth options to configure.</typeparam>
        /// <param name="name">The name of the OAuth provider to configure.</param>
        /// <param name="auth">The OAuth options to configure.</param>
        /// <param name="options">The <see cref="ExternalSignInOptions"/> to use to configure the instance.</param>
        private void ConfigureOAuth<T>(string name, T auth, ExternalSignInOptions options)
            where T : OAuthOptions
        {
            auth.ClientId = options.ClientId;
            auth.ClientSecret = options.ClientSecret;
            auth.Events = new OAuthEventsHandler(auth, _loggerFactory);

            ConfigureRemoteAuth(name, auth);
        }

        /// <summary>
        /// Configures an instance of <typeparamref name="T"/> for a remote authentication provider.
        /// </summary>
        /// <typeparam name="T">The type of the remote authentication options to configure.</typeparam>
        /// <param name="name">The name of the remote authentication provider to configure.</param>
        /// <param name="options">The remote authentication options to configure.</param>
        private void ConfigureRemoteAuth<T>(string name, T options)
            where T : RemoteAuthenticationOptions
        {
            options.Backchannel = _httpClientFactory.CreateClient(name);
            options.CorrelationCookie.Name = ApplicationCookie.Correlation.Name;
        }

        /// <summary>
        /// Tries to get the external sign-in settings for the specified provider.
        /// </summary>
        /// <param name="name">The name of the provider to get the provider settings for.</param>
        /// <param name="options">When the method returns, containsint the provider options, if enabled.</param>
        /// <returns>
        /// <see langword="true"/> if the specified provider is enabled; otherwise <see langword="false"/>.
        /// </returns>
        private bool TryGetProvider(string name, out ExternalSignInOptions options)
        {
            options = null;
            ExternalSignInOptions signInOptions = null;

            bool isEnabled =
                _options?.Authentication?.ExternalProviders?.TryGetValue(name, out signInOptions) == true &&
                signInOptions?.IsEnabled == true &&
                !string.IsNullOrEmpty(signInOptions?.ClientId) &&
                !string.IsNullOrEmpty(signInOptions?.ClientSecret);

            if (isEnabled)
            {
                options = signInOptions;
            }

            return isEnabled;
        }
    }
}
