// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using Azure.Security.KeyVault.Secrets;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class representing a builder to use to configure authorization for the application. This class cannot be inherited.
    /// </summary>
    public sealed class ApplicationAuthorizationBuilder
    {
        private readonly AuthenticationBuilder _builder;
        private readonly SiteOptions _options;
        private readonly Func<IServiceProvider> _serviceProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationAuthorizationBuilder"/> class.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to use.</param>
        /// <param name="options">The <see cref="SiteOptions"/> to use.</param>
        /// <param name="serviceProviderFactory">A delegate to a method that returns the <see cref="IServiceProvider"/> to use.</param>
        public ApplicationAuthorizationBuilder(
            AuthenticationBuilder builder,
            SiteOptions options,
            Func<IServiceProvider> serviceProviderFactory)
        {
            _builder = builder;
            _options = options;
            _serviceProviderFactory = serviceProviderFactory;
        }

        /// <summary>
        /// Gets the <see cref="AuthEvents"/> to use.
        /// </summary>
        private ExternalAuthEvents AuthEvents => _serviceProviderFactory().GetRequiredService<ExternalAuthEvents>();

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> to use.
        /// </summary>
        private IConfiguration Configuration => _serviceProviderFactory().GetRequiredService<IConfiguration>();

        /// <summary>
        /// Gets the <see cref="IHostEnvironment"/> to use.
        /// </summary>
        private IHostEnvironment Environment => _serviceProviderFactory().GetRequiredService<IHostEnvironment>();

        /// <summary>
        /// Gets the <see cref="IHttpClientFactory"/> to use.
        /// </summary>
        private IHttpClientFactory HttpClientFactory => _serviceProviderFactory().GetRequiredService<IHttpClientFactory>();

        /// <summary>
        /// Gets the <see cref="ILoggerFactory"/> to use.
        /// </summary>
        private ILoggerFactory LoggerFactory => _serviceProviderFactory().GetRequiredService<ILoggerFactory>();

        /// <summary>
        /// Gets the optional <see cref="SecretClient"/> to use.
        /// </summary>
        private SecretClient? SecretClient => _serviceProviderFactory().GetService<SecretClient>();

        /// <summary>
        /// Tries to configure Amazon authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddAmazon()
        {
            string name = "Amazon";

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddAmazon((auth) => ConfigureOAuth(name, auth, signInOptions!));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure Apple authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddApple()
        {
            string name = "Apple";
            string keyId = string.Empty;
            string teamId = string.Empty;

            ExternalSignInOptions? signInOptions = null;

            bool isEnabled =
                _options?.Authentication?.ExternalProviders?.TryGetValue(name, out signInOptions) == true &&
                signInOptions?.IsEnabled == true &&
                !string.IsNullOrEmpty(signInOptions.ClientId);

            if (isEnabled)
            {
                _builder.AddApple((options) =>
                {
                    options.ClientId = signInOptions!.ClientId;
                    options.KeyId = Configuration["Site:Authentication:ExternalProviders:Apple:KeyId"];
                    options.TeamId = Configuration["Site:Authentication:ExternalProviders:Apple:TeamId"];

                    var client = SecretClient;

                    if (client is null)
                    {
                        options.UsePrivateKey(
                            (keyId) =>
                                Environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
                    }
                    else
                    {
                        options.GenerateClientSecret = true;
                        options.PrivateKeyBytes = async (keyId) =>
                        {
                            var secret = await client.GetSecretAsync($"AuthKey-{keyId}");

                            string privateKey = secret.Value.Value;

                            if (privateKey.StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
                            {
                                string[] lines = privateKey.Split('\n');
                                privateKey = string.Join(string.Empty, lines[1..^1]);
                            }

                            return Convert.FromBase64String(privateKey);
                        };
                    }
                });
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

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddFacebook((auth) => ConfigureOAuth(name, auth, signInOptions!));
            }

            return this;
        }

        /// <summary>
        /// Tries to configure GitHub authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="ApplicationAuthorizationBuilder"/>.
        /// </returns>
        public ApplicationAuthorizationBuilder TryAddGitHub()
        {
            string name = "GitHub";

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddGitHub((auth) =>
                {
                    ConfigureOAuth(name, auth, signInOptions!);
                    auth.Scope.Add("user:email");
                });
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

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddGoogle((auth) => ConfigureOAuth(name, auth, signInOptions!));
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

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddMicrosoftAccount((auth) => ConfigureOAuth(name, auth, signInOptions!));
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

            if (TryGetProvider(name, out ExternalSignInOptions? signInOptions))
            {
                _builder.AddTwitter(
                    (options) =>
                    {
                        options.ConsumerKey = signInOptions!.ClientId;
                        options.ConsumerSecret = signInOptions.ClientSecret;
                        options.RetrieveUserDetails = true;
                        options.StateCookie.Name = ApplicationCookie.State.Name;

                        options.Events.OnRemoteFailure =
                            (context) => OAuthEventsHandler.HandleRemoteFailure(
                                context,
                                options.SignInScheme!,
                                options.StateDataFormat,
                                LoggerFactory.CreateLogger(name),
                                (token) => token?.Properties?.Items);

                        ConfigureRemoteAuth(name, options);

                        // Enable hook for integration tests, if configured
                        ExternalAuthEvents events = AuthEvents;

                        if (events?.OnRedirectToTwitterAuthorizationEndpoint != null)
                        {
                            options.Events.OnRedirectToAuthorizationEndpoint = events.OnRedirectToTwitterAuthorizationEndpoint;
                        }
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
            auth.Events = new OAuthEventsHandler(auth, AuthEvents, LoggerFactory);

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
            options.Backchannel = HttpClientFactory.CreateClient(name);
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
        private bool TryGetProvider(
            string name,
            [NotNullWhen(true)] out ExternalSignInOptions? options)
        {
            options = null;
            ExternalSignInOptions? signInOptions = null;

            bool isEnabled =
                _options?.Authentication?.ExternalProviders?.TryGetValue(name, out signInOptions) == true &&
                signInOptions?.IsEnabled == true &&
                !string.IsNullOrEmpty(signInOptions.ClientId) &&
                !string.IsNullOrEmpty(signInOptions.ClientSecret);

            if (isEnabled)
            {
                options = signInOptions;
            }

            return isEnabled;
        }
    }
}
