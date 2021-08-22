// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="HttpRequest"/> class. This class cannot be inherited.
/// </summary>
public static class HttpRequestExtensions
{
    /// <summary>
    /// Returns the canonical URI for the specified HTTP request with the optional path.
    /// </summary>
    /// <param name="request">The HTTP request to get the canonical URI from.</param>
    /// <param name="path">The optional path to get the canonical URI for.</param>
    /// <returns>
    /// The canonical URI to use for the specified HTTP request.
    /// </returns>
    public static string Canonical(this HttpRequest request, string? path = null)
    {
        string host = request.Host.ToString();
        string[] hostSplit = host.Split(':');

        var builder = new UriBuilder()
        {
            Host = hostSplit[0],
        };

        if (hostSplit.Length > 1)
        {
            builder.Port = int.Parse(hostSplit[1], CultureInfo.InvariantCulture);
        }

        builder.Path = path ?? request.Path;
        builder.Query = string.Empty;
        builder.Scheme = "https";

#pragma warning disable CA1308 // Normalize strings to uppercase
        string canonicalUri = builder.Uri.AbsoluteUri.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

        if (!canonicalUri.EndsWith("/", StringComparison.Ordinal))
        {
            canonicalUri += "/";
        }

        return canonicalUri;
    }
}
