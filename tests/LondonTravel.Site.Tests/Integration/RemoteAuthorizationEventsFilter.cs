// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Specialized;
    using System.Threading.Tasks;
    using System.Web;
    using MartinCostello.LondonTravel.Site.Identity;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A class representing a <see cref="IStartupFilter"/> that allows the tests to redirect external
    /// authentication provider requests back into the application. This class cannot be inherited.
    /// </summary>
    internal sealed class RemoteAuthorizationEventsFilter : IStartupFilter
    {
        private readonly Uri _serverAddress;

        public RemoteAuthorizationEventsFilter(Uri serverAddress)
        {
            _serverAddress = serverAddress;
        }

        /// <inheritdoc />
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return (builder) =>
            {
                next(builder);

                var events = builder.ApplicationServices.GetRequiredService<ExternalAuthEvents>();

                events.OnRedirectToOAuthAuthorizationEndpoint = RedirectToSelfForOAuth;
                events.OnRedirectToTwitterAuthorizationEndpoint = RedirectToSelfForTwitter;
            };
        }

        private static NameValueCollection ParseQueryString<T>(RedirectContext<T> context)
            where T : AuthenticationSchemeOptions
        {
            return HttpUtility.ParseQueryString(new UriBuilder(context.RedirectUri).Uri.Query);
        }

        private static Task Redirect<T>(RedirectContext<T> context, UriBuilder builder)
            where T : AuthenticationSchemeOptions
        {
            string location = builder.Uri.ToString();

            context.Response.Redirect(location);
            return Task.CompletedTask;
        }

        private static Task RedirectToSelfForOAuth<T>(RedirectContext<T> context)
            where T : AuthenticationSchemeOptions
        {
            NameValueCollection queryString = ParseQueryString(context);

            string? location = queryString["redirect_uri"];
            string? state = queryString["state"];

            queryString.Clear();

            string code = Guid.NewGuid().ToString();

            queryString.Add("code", code);
            queryString.Add("state", state);

            var builder = new UriBuilder(location!)
            {
                Query = queryString.ToString() ?? string.Empty,
            };

            return Redirect(context, builder);
        }

        private Task RedirectToSelfForTwitter<T>(RedirectContext<T> context)
            where T : AuthenticationSchemeOptions
        {
            NameValueCollection queryString = ParseQueryString(context);

            string? state = queryString["state"];

            queryString.Clear();

            string oauthToken = "twitter-oath-token"; // Fixed because the Twitter provider compares values internally
            string verifier = Guid.NewGuid().ToString();

            queryString.Add("state", state);
            queryString.Add("oauth_token", oauthToken);
            queryString.Add("oauth_verifier", verifier);

            var builder = new UriBuilder(_serverAddress)
            {
                Path = "/signin-twitter",
                Query = queryString.ToString() ?? string.Empty,
            };

            return Redirect(context, builder);
        }
    }
}
