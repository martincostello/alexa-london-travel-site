// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace MartinCostello.LondonTravel.Site.Extensions
{
    /// <summary>
    /// A class containing extension methods for the <see cref="ClaimsPrincipal"/> class. This class cannot be inherited.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// An array containing the space character. This field is read-only.
        /// </summary>
        private static readonly char[] Space = new[] { ' ' };

        /// <summary>
        /// Gets the URL of the avatar image to use for the specified claims principal.
        /// </summary>
        /// <param name="value">The user to get the avatar image URL for.</param>
        /// <param name="fallbackImageUrl">The URL of the default avatar image to use.</param>
        /// <param name="size">The preferred size of the avatar image.</param>
        /// <returns>
        /// The URL to use to render the avatar for the specified user.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> or <paramref name="fallbackImageUrl"/> is <see langword="null"/>.
        /// </exception>
        public static string GetAvatarUrl(this ClaimsPrincipal value, string fallbackImageUrl, int size = 24)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (fallbackImageUrl == null)
            {
                throw new ArgumentNullException(nameof(fallbackImageUrl));
            }

            string? email = value.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                return fallbackImageUrl;
            }

#pragma warning disable CA1308
            string normalized = email.Trim().ToLowerInvariant();
#pragma warning restore CA1308
            byte[] buffer = Encoding.UTF8.GetBytes(normalized);

#pragma warning disable CA5351
#pragma warning disable SCS0006
            byte[] hash = System.Security.Cryptography.MD5.HashData(buffer);
#pragma warning restore SCS0006
#pragma warning restore CA5351

            string hashString = HashToString(hash);

#pragma warning disable SYSLIB0013
            string escapedFallback = Uri.EscapeUriString(fallbackImageUrl);
#pragma warning restore SYSLIB0013

            return FormattableString.Invariant($"https://www.gravatar.com/avatar/{hashString}?s={size}&d={escapedFallback}");
        }

        /// <summary>
        /// Gets the display name to use for the specified claims principal.
        /// </summary>
        /// <param name="value">The user to get the display name for.</param>
        /// <returns>
        /// The display name to use for the specified user.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public static string GetDisplayName(this ClaimsPrincipal value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string[] givenNameClaims = { ClaimTypes.GivenName };

            foreach (string claim in givenNameClaims)
            {
                string? givenName = value.FindFirst(claim)?.Value;

                if (!string.IsNullOrWhiteSpace(givenName))
                {
                    return givenName.Trim();
                }
            }

            string? name = value.FindFirst((c) => c.Type == ClaimTypes.Name && c.Value != value.GetUserId() && c.Value != value.GetEmail())?.Value;

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim()
                    .Split(Space, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();
            }

            return name ?? string.Empty;
        }

        /// <summary>
        /// Gets the email address for the specified claims principal.
        /// </summary>
        /// <param name="value">The user to get the email address for.</param>
        /// <returns>
        /// The Id of the specified user.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public static string? GetEmail(this ClaimsPrincipal value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Gets the user Id for the specified claims principal.
        /// </summary>
        /// <param name="value">The user to get the Id for.</param>
        /// <returns>
        /// The Id of the specified user.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public static string? GetUserId(this ClaimsPrincipal value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Returns the string representation of the specified hash bytes.
        /// </summary>
        /// <param name="hash">An array containing the hash to convert to a string.</param>
        /// <returns>
        /// The string representation of <paramref name="hash"/>.
        /// </returns>
        private static string HashToString(byte[] hash)
        {
            var builder = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", hash[i]);
            }

            return builder.ToString();
        }
    }
}
