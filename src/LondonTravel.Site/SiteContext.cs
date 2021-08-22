// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class containing site contexts. This class cannot be inherited.
/// </summary>
public static class SiteContext
{
    /// <summary>
    /// The name of the property to store the site context error redirect URL in.
    /// </summary>
    public const string ErrorRedirectPropertyName = "london-travel-context-error-redirect";

    /// <summary>
    /// The context for linking an account.
    /// </summary>
    public const string LinkAccount = nameof(LinkAccount);

    /// <summary>
    /// The context for registering for an account.
    /// </summary>
    public const string Register = nameof(Register);

    /// <summary>
    /// The context for signing in to an account.
    /// </summary>
    public const string SignIn = nameof(SignIn);

    /// <summary>
    /// Sets the site context error redirect URL in the specified authentication properties.
    /// </summary>
    /// <param name="properties">The authentication properties to set the site context error redirect URL in.</param>
    /// <param name="value">The value for the site context.</param>
    public static void SetErrorRedirect(AuthenticationProperties properties, string value)
    {
        properties.Items[ErrorRedirectPropertyName] = value ?? "/";
    }
}
