// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration.Builders
{
    using System;
    using JustEat.HttpClientInterception;

    /// <summary>
    /// A class representing a builder for instances of <see cref="HttpRequestInterceptionBuilder"/>
    /// for requests to external social platforms for user profiles. This class cannot be inherited.
    /// </summary>
    public sealed class SocialProfileInterceptionBuilder
    {
        private string _displayName = "John Smith";
        private string _email = Guid.NewGuid().ToString() + "@john-smith.local";
        private string _facebookProfile = "john.smith";
        private string _firstName = "John";
        private string _fullName = "John Smith";
        private string _id = Guid.NewGuid().ToString();
        private string _middleName = "Jay";
        private string _surname = "Smith";

        /// <summary>
        /// Sets the display name to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithDisplayName(string value)
        {
            _displayName = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the email address to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithEmail(string value)
        {
            _email = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the Facebook profile to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithFacebookProfile(string value)
        {
            _facebookProfile = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the first/given name to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithFirstName(string value)
        {
            _firstName = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the full name to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithFullName(string value)
        {
            _fullName = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the user Id to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithId(string value)
        {
            _id = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the middle name to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithMiddleName(string value)
        {
            _middleName = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Sets the surname to use.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The current <see cref="SocialProfileInterceptionBuilder"/>.
        /// </returns>
        public SocialProfileInterceptionBuilder WithSurname(string value)
        {
            _surname = value ?? string.Empty;
            return this;
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Amazon API for a user's profile.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForAmazon()
        {
            var user = new
            {
                user_id = _id,
                name = _fullName,
                email = _email,
            };

            return Build("https://api.amazon.com/user/profile", user, ignoreQuery: true);
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Facebook Graph API for a user's profile.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForFacebook()
        {
            var user = new
            {
                id = _id,
                name = _fullName,
                first_name = _firstName,
                middle_name = _middleName,
                last_name = _surname,
                link = $"https://facebook.local/{_facebookProfile}",
                email = _email,
            };

            return Build($"https://graph.facebook.com/v{AccessTokensInterceptionBuilder.FacebookApiVersion}/me", user, ignoreQuery: true);
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Google+ API for a user's profile.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForGoogle()
        {
            var user = new
            {
                sub = _id,
                name = _displayName,
                given_name = _firstName,
                family_name = _surname,
                email = _email,
            };

            return Build("https://openidconnect.googleapis.com/v1/userinfo", user);
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Microsoft Graph API for a user's profile.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForMicrosoft()
        {
            var user = new
            {
                id = _id,
                displayName = _displayName,
                givenName = _firstName,
                surname = _surname,
                mail = _email,
            };

            return Build("https://graph.microsoft.com/v1.0/me", user);
        }

        /// <summary>
        /// Returns a new <see cref="HttpRequestInterceptionBuilder"/> that is configured
        /// for responding for requests to the Twitter API for a user's profile.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpRequestInterceptionBuilder"/> configured from the current instance.
        /// </returns>
        public HttpRequestInterceptionBuilder ForTwitter()
        {
            var user = new
            {
                email = _email,
            };

            return Build("https://api.twitter.com/1.1/account/verify_credentials.json?include_email=true", user);
        }

        private static HttpRequestInterceptionBuilder Build(string url, object user, bool ignoreQuery = false)
        {
            var builder = new HttpRequestInterceptionBuilder()
                .Requests().ForGet().ForUrl(url)
                .Responds().WithJsonContent(user);

            if (ignoreQuery)
            {
                builder.IgnoringQuery();
            }

            return builder;
        }
    }
}
