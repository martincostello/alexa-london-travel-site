// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A class containing extension methods for the <see cref="JavaScriptSnippet"/> class. This class cannot be inherited.
    /// </summary>
    public static class JavaScriptSnippetExtensions
    {
        /// <summary>
        /// The string to find to insert the CSP nonce attribute.
        /// </summary>
        private const string OpeningScriptTag = "<script ";

        /// <summary>
        /// Gets a code snippet with instrumentation key initialized from <see cref="TelemetryConfiguration"/>.
        /// </summary>
        /// <param name="snippet">The <see cref="JavaScriptSnippet"/> to use.</param>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>
        /// JavaScript code snippet with instrumentation key or empty if instrumentation key was not set for the application.
        /// </returns>
        public static string FullScriptWithCspNonce(this JavaScriptSnippet snippet, HttpContext context)
        {
            string script = snippet.FullScript;

            if (!string.IsNullOrEmpty(script))
            {
                int index = script.IndexOf(OpeningScriptTag, StringComparison.OrdinalIgnoreCase);

                if (index > -1)
                {
                    string nonce = context.EnsureCspNonce();
                    script = script.Insert(index + OpeningScriptTag.Length, $"nonce=\"{nonce}\" ");
                }
            }

            return script;
        }
    }
}
