// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration;

public sealed class ConfigureAuthenticationHandlers(IHttpClientFactory httpClientFactory) :
    IPostConfigureOptions<AmazonAuthenticationOptions>,
    IPostConfigureOptions<AppleAuthenticationOptions>,
    IPostConfigureOptions<GitHubAuthenticationOptions>,
    IPostConfigureOptions<GoogleOptions>,
    IPostConfigureOptions<MicrosoftAccountOptions>,
    IPostConfigureOptions<TwitterOptions>
{
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
