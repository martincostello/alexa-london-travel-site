// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using JustEat.HttpClientInterception;

    /// <summary>
    /// A class representing a builder for instances of <see cref="HttpRequestInterceptionBuilder"/>
    /// for requests to external social platforms for access tokens. This class cannot be inherited.
    /// </summary>
    public class AccessTokensInterceptionBuilder
    {
        internal const string FacebookApiVersion = "3.1";

        private static readonly Random _random = new Random();

        private string _accessToken = Guid.NewGuid().ToString();
        private string _refreshToken = Guid.NewGuid().ToString();
        private long _tokenLifetimeSeconds = 5 * 60;
        private string _tokenSecret = Guid.NewGuid().ToString();
        private string _tokenType = "access";
        private string _twitterScreenName = "@JohnSmith";
        private string _twitterUserId = _random.Next().ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets the access/OAuth token to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithAccessToken(string value)
        {
            _accessToken = value;
            return this;
        }

        /// <summary>
        /// Sets the refresh token to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithRefreshToken(string value)
        {
            _refreshToken = value;
            return this;
        }

        /// <summary>
        /// Sets the token lifetime, in seconds, to use.
        /// </summary>
        /// <param name="valueInSeconds">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithTokenLifetime(long valueInSeconds)
        {
            _tokenLifetimeSeconds = valueInSeconds;
            return this;
        }

        /// <summary>
        /// Sets the token secret to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithTokenSecret(string value)
        {
            _tokenSecret = value;
            return this;
        }

        /// <summary>
        /// Sets the token type to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithTokenType(string value)
        {
            _tokenType = value;
            return this;
        }

        /// <summary>
        /// Sets the Twitter screen name (handle) to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithTwitterScreenName(string value)
        {
            _twitterScreenName = value;
            return this;
        }

        /// <summary>
        /// Sets the Twitter user Id to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="AccessTokensInterceptionBuilder"/>.
        /// </returns>
        public AccessTokensInterceptionBuilder WithTwitterUserId(string value)
        {
            _twitterUserId = value;
            return this;
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Amazon API for access tokens.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForAmazon() => ForOAuth("https://api.amazon.com/auth/o2/token");

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Facebook Graph API for access tokens.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForFacebook() => ForOAuth($"https://graph.facebook.com/v{FacebookApiVersion}/oauth/access_token");

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Google OAuth API for access tokens.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForGoogle() => ForOAuth("https://www.googleapis.com/oauth2/v4/token");

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Microsoft Account API for access tokens.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForMicrosoft() => ForOAuth("https://login.microsoftonline.com/common/oauth2/v2.0/token");

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Twitter API for an access token.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForTwitterAccessToken()
        {
            var fields = new Dictionary<string, string>()
            {
                { "user_id", _twitterUserId ?? string.Empty },
                { "screen_name", _twitterScreenName ?? string.Empty },
            };

            return ForTwitter("https://api.twitter.com/oauth/access_token", fields);
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Twitter API for a request token.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForTwitterRequestToken()
        {
            var fields = new Dictionary<string, string>()
            {
                { "oauth_callback_confirmed", "true" },
            };

            return ForTwitter("https://api.twitter.com/oauth/request_token", fields);
        }

        private object BuildOAuthTokens()
        {
            return new
            {
                access_token = _accessToken ?? string.Empty,
                token_type = _tokenType ?? string.Empty,
                refresh_token = _refreshToken ?? string.Empty,
                expires_in = _tokenLifetimeSeconds.ToString(CultureInfo.InvariantCulture),
            };
        }

        private HttpRequestInterceptionBuilder ForOAuth(string url)
        {
            var tokens = BuildOAuthTokens();

            return new HttpRequestInterceptionBuilder()
                .Requests().ForPost().ForUrl(url)
                .Responds().WithJsonContent(tokens);
        }

        private HttpRequestInterceptionBuilder ForTwitter(string url, IDictionary<string, string> parameters)
        {
            var form = new Dictionary<string, string>()
            {
                { "oauth_token", _accessToken ?? string.Empty },
                { "oauth_token_secret", _tokenSecret ?? string.Empty },
            };

            foreach (var pair in parameters)
            {
                form[pair.Key] = pair.Value;
            }

            return new HttpRequestInterceptionBuilder()
                .Requests().ForPost().ForUrl(url)
                .Responds().WithFormContent(form);
        }
    }
}
