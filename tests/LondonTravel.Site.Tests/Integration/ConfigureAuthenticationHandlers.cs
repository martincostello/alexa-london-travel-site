// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Specialized;
using System.Web;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using MartinCostello.LondonTravel.Site.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration
{
    public sealed class ConfigureAuthenticationHandlers :
        IPostConfigureOptions<AmazonAuthenticationOptions>,
        IPostConfigureOptions<AppleAuthenticationOptions>,
        IPostConfigureOptions<FacebookOptions>,
        IPostConfigureOptions<GitHubAuthenticationOptions>,
        IPostConfigureOptions<GoogleOptions>,
        IPostConfigureOptions<MicrosoftAccountOptions>,
        IPostConfigureOptions<TwitterOptions>
    {
        public ConfigureAuthenticationHandlers(
            IHost host,
            IHttpClientFactory httpClientFactory)
        {
            Host = host;
            HttpClientFactory = httpClientFactory;
        }

        private IHost Host { get; }

        private IHttpClientFactory HttpClientFactory { get; }

        public void PostConfigure(string name, AmazonAuthenticationOptions options) => Configure(name, options);

        public void PostConfigure(string name, AppleAuthenticationOptions options) => Configure(name, options);

        public void PostConfigure(string name, FacebookOptions options) => Configure(name, options);

        public void PostConfigure(string name, GitHubAuthenticationOptions options) => Configure(name, options);

        public void PostConfigure(string name, GoogleOptions options) => Configure(name, options);

        public void PostConfigure(string name, MicrosoftAccountOptions options) => Configure(name, options);

        public void PostConfigure(string name, TwitterOptions options)
        {
            options.Backchannel = HttpClientFactory.CreateClient(name);
            options.Events.OnRedirectToAuthorizationEndpoint = RedirectToSelfForTwitter;
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

            var builder = new UriBuilder(Host.GetAddress())
            {
                Path = "/signin-twitter",
                Query = queryString.ToString() ?? string.Empty,
            };

            return Redirect(context, builder);
        }

        private void Configure<T>(string name, T options)
            where T : OAuthOptions
        {
            options.Backchannel = HttpClientFactory.CreateClient(name);
            options.Events.OnRedirectToAuthorizationEndpoint = RedirectToSelfForOAuth;
        }
    }
}
