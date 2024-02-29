// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IUrlHelper"/> class. This class cannot be inherited.
/// </summary>
public static class IUrlHelperExtensions
{
    /// <summary>
    /// Converts a virtual (relative) path to an application absolute URI.
    /// </summary>
    /// <param name="value">The <see cref="IUrlHelper"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <returns>The application absolute URI.</returns>
    public static string AbsoluteContent(this IUrlHelper value, string contentPath)
    {
        var request = value.ActionContext.HttpContext.Request;
        return value.ToAbsolute(request.Host.Value ?? string.Empty, contentPath);
    }

    /// <summary>
    /// Converts a virtual (relative) path to an CDN absolute URI, if configured.
    /// </summary>
    /// <param name="value">The <see cref="IUrlHelper"/>.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <param name="options">The current site configuration.</param>
    /// <param name="appendVersion">Whether to append a version query string parameter to the URL.</param>
    /// <returns>The CDN absolute URI, if configured; otherwise the application absolute URI..</returns>
    public static string CdnContent(this IUrlHelper value, string contentPath, SiteOptions? options, bool appendVersion = true)
    {
        var cdn = options?.ExternalLinks?.Cdn;

        // Prefer empty images to a NullReferenceException
        if (cdn == null)
        {
            return string.Empty;
        }

        // Azure Blob storage is case-sensitive, so force all URLs to lowercase
#pragma warning disable CA1308
        string url = value.ToAbsolute(cdn.Host, $"london-travel_{contentPath.ToLowerInvariant().TrimStart('/')}");
#pragma warning restore CA1308

        // asp-append-version="true" does not work for non-local resources
        if (appendVersion)
        {
            url += $"?v={GitMetadata.Commit}";
        }

        return url;
    }

    /// <summary>
    /// Converts a virtual (relative) path to an absolute URI.
    /// </summary>
    /// <param name="value">The <see cref="IUrlHelper"/>.</param>
    /// <param name="host">The current host.</param>
    /// <param name="contentPath">The virtual path of the content.</param>
    /// <returns>The application absolute URI.</returns>
    private static string ToAbsolute(this IUrlHelper value, string host, string contentPath)
    {
        var request = value.ActionContext.HttpContext.Request;
        return new Uri(new Uri(request.Scheme + "://" + host), value.Content(contentPath)).ToString();
    }
}
