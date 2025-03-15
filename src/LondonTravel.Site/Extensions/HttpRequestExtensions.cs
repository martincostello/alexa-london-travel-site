// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;

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
        builder.Scheme = Uri.UriSchemeHttps;

#pragma warning disable CA1308
        string canonicalUri = builder.Uri.AbsoluteUri.ToLowerInvariant();
#pragma warning restore CA1308

        if (!canonicalUri.EndsWith('/'))
        {
            canonicalUri += "/";
        }

        return canonicalUri;
    }

    /// <summary>
    /// Converts a virtual (relative) path to an CDN absolute URI, if configured.
    /// </summary>
    /// <param name="value">The <see cref="HttpRequest"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="options">The current site configuration.</param>
    /// <returns>The CDN absolute URI, if configured; otherwise the application absolute URI.</returns>
    public static string CdnContent(this HttpRequest value, string contentPath, SiteOptions options)
    {
        var cdn = options?.ExternalLinks?.Cdn;

        // Prefer empty images to a NullReferenceException
        if (cdn == null)
        {
            return string.Empty;
        }

        return $"{cdn}london-travel_{value.Content(contentPath)?.TrimStart('/')}";
    }

    /// <summary>
    /// Converts a virtual (relative) path to a relative URI.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="appendVersion">Whether to append a version to the URL.</param>
    /// <returns>The relatve URI to the content.</returns>
    public static string? Content(this HttpRequest request, string? contentPath, bool appendVersion = true)
    {
        string? result = string.Empty;

        if (!string.IsNullOrEmpty(contentPath))
        {
            if (contentPath[0] == '~')
            {
                var segment = new PathString(contentPath[1..]);
                var applicationPath = request.PathBase;

                var path = applicationPath.Add(segment);
                result = path.Value;
            }
            else
            {
                result = contentPath;
            }
        }

        if (appendVersion)
        {
            result += $"?v={GitMetadata.Commit}";
        }

        return result;
    }
}
