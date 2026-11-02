// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

/// <summary>
/// A class containing helper methods for the account pages. This class cannot be inherited.
/// </summary>
internal static class AccountHelpers
{
    /// <summary>
    /// The path of the endpoint used to authorize the Alexa skill.
    /// </summary>
    private const string AlexaAuthorizationPath = "/alexa/authorize";

    /// <summary>
    /// Returns whether sign-in with at least one external provider is enabled.
    /// </summary>
    /// <param name="options">The current site options.</param>
    /// <returns>
    /// <see langword="true"/> if sign-in is enabled; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsSignInEnabled(SiteOptions? options)
    {
        return
            options?.Authentication?.IsEnabled == true &&
            options.Authentication.ExternalProviders?.Any((p) => p.Value?.IsEnabled == true) == true;
    }

    /// <summary>
    /// Returns whether the specified URL is for the endpoint to authorize the Alexa skill.
    /// </summary>
    /// <param name="url">The URL to test.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="url"/> is for the Alexa skill
    /// authorization endpoint; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsAlexaAuthorization(string? url)
        => IsPath(url, AlexaAuthorizationPath);

    /// <summary>
    /// Returns whether the specified URL has the specified path.
    /// </summary>
    /// <param name="url">The URL to test.</param>
    /// <param name="path">The path to test for.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="url"/> has the path specified
    /// by <paramref name="path"/>; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsPath(string? url, string? path)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            string.IsNullOrWhiteSpace(path) ||
            !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return false;
        }

        if (uri.IsAbsoluteUri)
        {
            return string.Equals(uri.AbsolutePath, path, StringComparison.OrdinalIgnoreCase);
        }

        int indexOfQuery = url.IndexOf('?', StringComparison.Ordinal);

        if (indexOfQuery > -1)
        {
            url = url[..indexOfQuery];
        }

        return string.Equals(url.TrimEnd('/'), path.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }
}
