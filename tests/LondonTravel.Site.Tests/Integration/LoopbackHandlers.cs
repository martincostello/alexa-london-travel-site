// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Specialized;
using System.Web;
using MartinCostello.LondonTravel.Site.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Site.Integration;

internal static class LoopbackHandlers
{
    public static Task Configure(RedirectContext<OAuthOptions> context)
    {
        var queryString = ParseQueryString(context);

        string? location = queryString["redirect_uri"];
        string? state = queryString[nameof(state)];

        queryString.Clear();

        string code = Guid.NewGuid().ToString();

        queryString.Add(nameof(code), code);
        queryString.Add(nameof(state), state);

        var builder = new UriBuilder(location!)
        {
            Query = queryString.ToString() ?? string.Empty,
        };

        context.Response.Redirect(builder.Uri.ToString());

        return Task.CompletedTask;
    }

    public static Task Configure(RedirectContext<TwitterOptions> context)
    {
        var queryString = ParseQueryString(context);

        string? state = queryString[nameof(state)];

        queryString.Clear();

        string oauthToken = "twitter-oath-token"; // Fixed because the Twitter provider compares values internally
        string verifier = Guid.NewGuid().ToString();

        queryString.Add(nameof(state), state);
        queryString.Add("oauth_token", oauthToken);
        queryString.Add("oauth_verifier", verifier);

        var host = context.HttpContext.RequestServices.GetRequiredService<IServer>();

        var builder = new UriBuilder(host.GetAddress()!)
        {
            Path = context.Options.CallbackPath,
            Query = queryString.ToString() ?? string.Empty,
        };

        context.Response.Redirect(builder.Uri.ToString());

        return Task.CompletedTask;
    }

    private static NameValueCollection ParseQueryString<T>(RedirectContext<T> context)
        where T : AuthenticationSchemeOptions
    {
        return HttpUtility.ParseQueryString(new UriBuilder(context.RedirectUri).Uri.Query);
    }
}
