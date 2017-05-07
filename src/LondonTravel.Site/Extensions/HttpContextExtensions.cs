// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Security.Cryptography;
    using Microsoft.AspNetCore.Http;

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
        /// Gets the CSP nonce value for the current HTTP context, generating one if not already set.
        /// </summary>
        /// <param name="context">The HTTP context to get the CSP nonce from.</param>
        /// <returns>
        /// The current CSP nonce value, if any.
        /// </returns>
        public static string EnsureCspNonce(this HttpContext context)
        {
            string nonce = context.GetCspNonce();

            if (nonce == null)
            {
                using (var random = RandomNumberGenerator.Create())
                {
                    byte[] data = new byte[32];
                    random.GetBytes(data);

                    nonce = Convert.ToBase64String(data).Replace("+", "/"); // '+' causes encoding issues with TagHelpers
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
        public static string GetCspNonce(this HttpContext context)
        {
            string nonce = null;

            if (context.Items.TryGetValue(CspNonceKey, out object value))
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
    }
}
