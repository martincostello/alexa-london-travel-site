// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Builders
{
    using JustEat.HttpClientInterception;

    /// <summary>
    /// A class representing a builder for instances of <see cref="HttpRequestInterceptionBuilder"/>
    /// for requests to external social platforms for authentication. This class cannot be inherited.
    /// </summary>
    public class AuthenticationInterceptionBuilder
    {
        private AccessTokensInterceptionBuilder _tokens;
        private SocialProfileInterceptionBuilder _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationInterceptionBuilder"/> class.
        /// </summary>
        /// <param name="options">The <see cref="HttpClientInterceptorOptions"/> to use.</param>
        public AuthenticationInterceptionBuilder(HttpClientInterceptorOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Gets the <see cref="HttpClientInterceptorOptions"/> in use.
        /// </summary>
        public HttpClientInterceptorOptions Options { get; }

        /// <summary>
        /// Gets the <see cref="AccessTokensInterceptionBuilder"/> to use to configure access tokens.
        /// </summary>
        /// <returns>
        /// The <see cref="AccessTokensInterceptionBuilder"/> to use.
        /// </returns>
        public AccessTokensInterceptionBuilder Tokens() => _tokens = _tokens ?? new AccessTokensInterceptionBuilder();

        /// <summary>
        /// Gets the <see cref="SocialProfileInterceptionBuilder"/> to use to configure users.
        /// </summary>
        /// <returns>
        /// The <see cref="SocialProfileInterceptionBuilder"/> to use.
        /// </returns>
        public SocialProfileInterceptionBuilder User() => _user = _user ?? new SocialProfileInterceptionBuilder();

        /// <summary>
        /// Configures <see cref="Options"/> for Amazon authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="AuthenticationInterceptionBuilder"/>.
        /// </returns>
        public AuthenticationInterceptionBuilder ForAmazon()
        {
            Register(Tokens().ForAmazon(), User().ForAmazon());
            return this;
        }

        /// <summary>
        /// Configures <see cref="Options"/> for Facebook authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="AuthenticationInterceptionBuilder"/>.
        /// </returns>
        public AuthenticationInterceptionBuilder ForFacebook()
        {
            Register(Tokens().ForFacebook(), User().ForFacebook());
            return this;
        }

        /// <summary>
        /// Configures <see cref="Options"/> for Google authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="AuthenticationInterceptionBuilder"/>.
        /// </returns>
        public AuthenticationInterceptionBuilder ForGoogle()
        {
            Register(Tokens().ForGoogle(), User().ForGoogle());
            return this;
        }

        /// <summary>
        /// Configures <see cref="Options"/> for Microsoft authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="AuthenticationInterceptionBuilder"/>.
        /// </returns>
        public AuthenticationInterceptionBuilder ForMicrosoft()
        {
            Register(Tokens().ForMicrosoft(), User().ForMicrosoft());
            return this;
        }

        /// <summary>
        /// Configures <see cref="Options"/> for Twitter authentication.
        /// </summary>
        /// <returns>
        /// The current <see cref="AuthenticationInterceptionBuilder"/>.
        /// </returns>
        public AuthenticationInterceptionBuilder ForTwitter()
        {
            var tokens = Tokens();

            Register(
                tokens.ForTwitterAccessToken(),
                tokens.ForTwitterRequestToken(),
                User().ForTwitter());

            return this;
        }

        private void Register(params HttpRequestInterceptionBuilder[] builders)
        {
            foreach (var builder in builders)
            {
                builder.RegisterWith(Options);
            }
        }
    }
}
