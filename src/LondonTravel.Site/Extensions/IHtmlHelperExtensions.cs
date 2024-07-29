// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc.Rendering;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IHtmlHelper"/> interface. This class cannot be inherited.
/// </summary>
public static class IHtmlHelperExtensions
{
    /// <summary>
    /// A mapping of authentication schemes to icon classes. This field is read-only.
    /// </summary>
    private static readonly Dictionary<string, string> _iconImageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Amazon"] = "amazon",
        ["Apple"] = "apple",
        ["GitHub"] = "github",
        ["Google"] = "google",
        ["Microsoft"] = "windows",
        ["Twitter"] = "twitter",
    };

    /// <summary>
    /// Gets the CSS class to use for a social login button.
    /// </summary>
    /// <param name="html">The <see cref="IHtmlHelper"/> to use.</param>
    /// <param name="authenticationScheme">The Id/name of the authentication scheme.</param>
    /// <returns>
    /// The CSS class to use for a button for the authentication scheme.
    /// </returns>
    public static string GetSocialLoginButtonCss(this IHtmlHelper html, string authenticationScheme)
    {
        ArgumentNullException.ThrowIfNull(html);

#pragma warning disable CA1308
        return $"btn-{authenticationScheme?.ToLowerInvariant()}";
#pragma warning restore CA1308
    }

    /// <summary>
    /// Gets the CSS class to use for a social login icon.
    /// </summary>
    /// <param name="html">The <see cref="IHtmlHelper"/> to use.</param>
    /// <param name="authenticationScheme">The Id/name of the authentication scheme.</param>
    /// <returns>
    /// The CSS class to use for an icon for the authentication scheme.
    /// </returns>
    public static string GetSocialLoginIconCss(this IHtmlHelper html, string authenticationScheme)
    {
        ArgumentNullException.ThrowIfNull(html);

        if (string.IsNullOrEmpty(authenticationScheme))
        {
            return string.Empty;
        }

        string iconClass = string.Empty;

        if (_iconImageMap.TryGetValue(authenticationScheme, out string? modifier))
        {
            iconClass = $"fa-{modifier}";
        }

        return iconClass;
    }
}
