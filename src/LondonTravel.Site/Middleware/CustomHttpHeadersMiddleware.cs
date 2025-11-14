// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace MartinCostello.LondonTravel.Site.Middleware;

/// <summary>
/// A class representing middleware for adding custom HTTP response headers. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CustomHttpHeadersMiddleware"/> class.
/// </remarks>
/// <param name="next">The delegate for the next part of the pipeline.</param>
/// <param name="environment">The current hosting environment.</param>
/// <param name="config">The current configuration.</param>
/// <param name="options">The current site configuration options.</param>
public sealed class CustomHttpHeadersMiddleware(
    RequestDelegate next,
    IWebHostEnvironment environment,
    IConfiguration config,
    IOptionsMonitor<SiteOptions> options)
{
    /// <summary>
    /// The options snapshot to use. This field is read-only.
    /// </summary>
    private readonly IOptionsMonitor<SiteOptions> _options = options;

    /// <summary>
    /// The current <c>Expect-CT</c> HTTP response header value. This field is read-only.
    /// </summary>
    private readonly string _expectCTValue = BuildExpectCT(options.CurrentValue);

    /// <summary>
    /// The name of the current hosting environment. This field is read-only.
    /// </summary>
    private readonly string _environmentName = config.AzureEnvironment();

    /// <summary>
    /// Whether the current hosting environment is production. This field is read-only.
    /// </summary>
    private readonly bool _isProduction = environment.IsProduction();

    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the actions performed by the middleware.
    /// </returns>
    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove(HeaderNames.Server);
                context.Response.Headers.Remove(HeaderNames.XPoweredBy);

                context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "unsafe-none");
                context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
                context.Response.Headers.Append("Cross-Origin-Resource-Policy", "same-origin");
                context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.Append("X-Download-Options", "noopen");
                context.Response.Headers.XXSSProtection = "1; mode=block";

                // Middleware (e.g. Identity) may have already set X-Frame-Options
                context.Response.Headers.XFrameOptions = "DENY";

                string? nonce = context.GetCspNonce();

                if (nonce != null)
                {
                    context.Response.Headers.Append("x-csp-nonce", nonce);
                }

                bool allowInlineStyles = context.InlineStylesAllowed();

                string csp = BuildContentSecurityPolicy(nonce, allowInlineStyles, isReport: false);
                string cspReport = BuildContentSecurityPolicy(nonce, allowInlineStyles, isReport: true);

                context.Response.Headers.ContentSecurityPolicy = csp;
                context.Response.Headers.ContentSecurityPolicyReportOnly = cspReport;

                if (context.Request.IsHttps)
                {
                    context.Response.Headers.Append("Expect-CT", _expectCTValue);
                }

                context.Response.Headers.Append("NEL", /*lang=json,strict*/@"{""report_to"":""default"",""max_age"":31536000,""include_subdomains"":false}");

#if DEBUG
                context.Response.Headers.Append("X-Debug", "true");
#endif

                if (_environmentName != null)
                {
                    context.Response.Headers.Append("X-Environment", _environmentName);
                }

                context.Response.Headers.Append("X-Instance", Environment.MachineName);
                context.Response.Headers.Append("X-Request-Id", context.TraceIdentifier);
                context.Response.Headers.Append("X-Revision", GitMetadata.Commit);
                context.Response.Headers.XUACompatible = "IE=edge";

                if (!context.Response.Headers.ContainsKey(HeaderNames.Pragma))
                {
                    context.Response.Headers.Pragma = "no-cache";
                }

                return Task.CompletedTask;
            });

        await next(context);
    }

    /// <summary>
    /// Builds the value to use for the <c>Expect-CT</c> HTTP response header.
    /// </summary>
    /// <param name="options">The current site configuration options.</param>
    /// <returns>
    /// A <see cref="string"/> containing the <c>Expect-CT</c> value to use.
    /// </returns>
    private static string BuildExpectCT(SiteOptions options)
    {
        if (options == null)
        {
            return string.Empty;
        }
        
        var builder = new StringBuilder();

        bool enforce = options.CertificateTransparency?.Enforce is true;

        if (enforce)
        {
            builder.Append("enforce; ");
        }

        builder.AppendFormat(
            CultureInfo.InvariantCulture,
            "max-age={0};",
            (int)(options.CertificateTransparency?.MaxAge.TotalSeconds ?? default));

        if (enforce && options.ExternalLinks?.Reports?.ExpectCTEnforce is { } enforceUri)
        {
            builder.Append(" report-uri ");
            builder.Append(enforceUri);
        }
        else if (options?.ExternalLinks?.Reports?.ExpectCTReportOnly is { } reportUri)
        {
            builder.Append(" report-uri ");
            builder.Append(reportUri);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the CDN origin to use for the Content Security Policy.
    /// </summary>
    /// <param name="options">The current site options.</param>
    /// <returns>
    /// The origin to use for the CDN, if any.
    /// </returns>
    private static string GetCdnOriginForContentSecurityPolicy(SiteOptions options)
    {
        return GetOriginForContentSecurityPolicy(options?.ExternalLinks?.Cdn);
    }

    /// <summary>
    /// Gets the origin to use for the Content Security Policy from the specified URI.
    /// </summary>
    /// <param name="baseUri">The base URI to get the origin for.</param>
    /// <returns>
    /// The origin to use for the URI, if any.
    /// </returns>
    private static string GetOriginForContentSecurityPolicy(Uri? baseUri)
    {
        if (baseUri == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(baseUri.Host);

        if (!baseUri.IsDefaultPort)
        {
            builder.Append(':');
            builder.Append(baseUri.Port);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds the Content Security Policy to use for the website.
    /// </summary>
    /// <param name="nonce">The nonce value to use, if any.</param>
    /// <param name="allowInlineStyles">Whether to allow the use of inline styles.</param>
    /// <param name="isReport">Whether the policy is being generated for the report.</param>
    /// <returns>
    /// A <see cref="string"/> containing the Content Security Policy to use.
    /// </returns>
    private string BuildContentSecurityPolicy(string? nonce, bool allowInlineStyles, bool isReport)
    {
        var options = _options.CurrentValue;
        string? cdn = GetCdnOriginForContentSecurityPolicy(options);

        List<string> scriptDirectives =
        [
            Csp.Self,
        ];

        List<string> styleDirectives =
        [
            Csp.Self,
        ];

        var policies = new Dictionary<string, IList<string>>()
        {
            ["default-src"] = [Csp.Self, Csp.Data],
            ["script-src"] = scriptDirectives,
            ["style-src"] = styleDirectives,
            ["img-src"] = [Csp.Self, Csp.Data, cdn],
            ["font-src"] = [Csp.Self],
            ["connect-src"] = [Csp.Self],
            ["media-src"] = [Csp.None],
            ["object-src"] = [Csp.None],
            ["child-src"] = [Csp.Self],
            ["frame-ancestors"] = [Csp.None],
            ["form-action"] = [Csp.Self],
            ["block-all-mixed-content"] = [],
            ["base-uri"] = [Csp.Self],
            ["manifest-src"] = [Csp.Self],
            ["worker-src"] = [Csp.Self],
        };

        if (allowInlineStyles)
        {
            styleDirectives.Add(Csp.UnsafeInline);
        }

        if (nonce != null)
        {
            string nonceDirective = $"'nonce-{nonce}'";

            scriptDirectives.Add(nonceDirective);

            // Unsafe inline does not work if a nonce is present
            if (!allowInlineStyles)
            {
                styleDirectives.Add(nonceDirective);
            }
        }
        else if (!_isProduction)
        {
            // Fix developer exception pages
            scriptDirectives.Add(Csp.UnsafeInline);

            // Prevent duplicate directives
            if (!styleDirectives.Contains(Csp.UnsafeInline))
            {
                styleDirectives.Add(Csp.UnsafeInline);
            }
        }

        var builder = new StringBuilder();

        foreach (var pair in policies)
        {
            builder.Append(pair.Key);

            var origins = pair.Value;

            if (options.ContentSecurityPolicyOrigins != null &&
                options.ContentSecurityPolicyOrigins.TryGetValue(pair.Key, out var configOrigins))
            {
                origins = [.. origins.Concat(configOrigins)];
            }

            origins = [.. origins.Where((p) => !string.IsNullOrWhiteSpace(p)).Distinct()];

            if (origins.Count > 0)
            {
                builder.Append(' ');
                builder.Append(string.Join(' ', origins));
            }

            builder.Append(';');
        }

        if (!isReport && _isProduction)
        {
            builder.Append("upgrade-insecure-requests;");
        }

        if (options?.ExternalLinks?.Reports?.ContentSecurityPolicy is { } reportUri)
        {
            builder.Append(CultureInfo.InvariantCulture, $"report-uri {reportUri};");
        }

        return builder.ToString();
    }

    /// <summary>
    /// A class containing Content Security Policy constants.
    /// </summary>
    private static class Csp
    {
        /// <summary>
        /// The origin for a data URI.
        /// </summary>
        internal const string Data = "data:";

        /// <summary>
        /// The directive to allow no origins.
        /// </summary>
        internal const string None = "'none'";

        /// <summary>
        /// The origin for the site itself.
        /// </summary>
        internal const string Self = "'self'";

        /// <summary>
        /// The directive to allow inline content.
        /// </summary>
        internal const string UnsafeInline = "'unsafe-inline'";
    }
}
