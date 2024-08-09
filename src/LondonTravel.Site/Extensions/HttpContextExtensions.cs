// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="HttpContext"/> class. This class cannot be inherited.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// The name of the key for storing the CSP nonce.
    /// </summary>
    private const string CspNonceKey = "x-csp-nonce";

    /// <summary>
    /// The name of the key for allowing inline use of styles.
    /// </summary>
    private const string InlineStylesKey = "x-styles-unsafe-inline";

    /// <summary>
    /// Gets the CSP nonce value for the current HTTP context, generating one if not already set.
    /// </summary>
    /// <param name="context">The HTTP context to get the CSP nonce from.</param>
    /// <returns>
    /// The current CSP nonce value, if any.
    /// </returns>
    public static string EnsureCspNonce(this HttpContext context)
    {
        string? nonce = context.GetCspNonce();

        if (nonce == null)
        {
            byte[] data = [];

            try
            {
                data = RandomNumberGenerator.GetBytes(32);
                nonce = System.Buffers.Text.Base64Url.EncodeToString(data);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(data);
            }

            context.SetCspNonce(nonce);
        }

        return nonce;
    }

    /// <summary>
    /// Gets the CSP nonce value, if any, for the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context to get the CSP nonce from.</param>
    /// <returns>
    /// The current CSP nonce value, if any.
    /// </returns>
    public static string? GetCspNonce(this HttpContext context)
    {
        string? nonce = null;

        if (context.Items.TryGetValue(CspNonceKey, out object? value))
        {
            nonce = value as string;
        }

        return nonce;
    }

    /// <summary>
    /// Sets the CSP nonce value for the current HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context to set the CSP nonce for.</param>
    /// <param name="nonce">The value of the CSP nonce.</param>
    public static void SetCspNonce(this HttpContext context, string nonce)
    {
        context.Items[CspNonceKey] = nonce;
    }

    /// <summary>
    /// Gets a value indicating whether inline styles may be used in the CSP.
    /// </summary>
    /// <param name="context">The HTTP context to get the value from.</param>
    /// <returns>
    /// <see langword="true"/> if inline styles may be used; otherwise <see langword="false"/>.
    /// </returns>
    public static bool InlineStylesAllowed(this HttpContext context)
    {
        bool result = false;

        if (context.Items.TryGetValue(InlineStylesKey, out object? value) &&
            value is bool allowInlineStyles)
        {
            result = allowInlineStyles;
        }

        return result;
    }

    /// <summary>
    /// Sets that inline styles can be used for this HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context to allow inline styles for.</param>
    public static void AllowInlineStyles(this HttpContext context)
    {
        context.Items[InlineStylesKey] = true;
    }
}
