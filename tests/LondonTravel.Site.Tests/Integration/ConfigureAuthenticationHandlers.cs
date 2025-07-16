// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration;

public sealed class ConfigureAuthenticationHandlers(IHttpClientFactory httpClientFactory, IServer server) :
    IPostConfigureOptions<AmazonAuthenticationOptions>,
    IPostConfigureOptions<AppleAuthenticationOptions>,
    IPostConfigureOptions<GitHubAuthenticationOptions>,
    IPostConfigureOptions<GoogleOptions>,
    IPostConfigureOptions<MicrosoftAccountOptions>,
    IPostConfigureOptions<SiteOptions>,
    IPostConfigureOptions<TwitterOptions>
{
    private string? _redirectUrl;

    public void PostConfigure(string? name, SiteOptions options)
    {
        // Allow the tests on the self-hosted server to link accounts via "Amazon"
        if (_redirectUrl is null && server.GetAddress() is { } uri)
        {
            _redirectUrl = new Uri(uri, "manage/").ToString();

            options.Alexa ??= new();
            options.Alexa.RedirectUrls ??= [];
            options.Alexa.RedirectUrls.Add(_redirectUrl);
        }
    }

    public void PostConfigure(string? name, AmazonAuthenticationOptions options)
        => Configure(name, options);

    public void PostConfigure(string? name, AppleAuthenticationOptions options)
        => Configure(name, options);

    public void PostConfigure(string? name, GitHubAuthenticationOptions options)
        => Configure(name, options);

    public void PostConfigure(string? name, GoogleOptions options)
        => Configure(name, options);

    public void PostConfigure(string? name, MicrosoftAccountOptions options)
        => Configure(name, options);

    public void PostConfigure(string? name, TwitterOptions options)
    {
        options.Backchannel = httpClientFactory.CreateClient(name ?? string.Empty);
        options.Events.OnRedirectToAuthorizationEndpoint = LoopbackHandlers.Configure;
    }

    private void Configure<TOptions>(string? name, TOptions options)
        where TOptions : OAuthOptions
    {
        options.Backchannel = httpClientFactory.CreateClient(name ?? string.Empty);
        options.Events.OnRedirectToAuthorizationEndpoint = LoopbackHandlers.Configure;
    }
}
