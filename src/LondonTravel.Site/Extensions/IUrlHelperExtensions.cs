// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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
