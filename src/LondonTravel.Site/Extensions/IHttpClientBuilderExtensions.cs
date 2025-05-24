// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing extension methods for the <see cref="IHttpClientBuilder"/> interface. This class cannot be inherited.
/// </summary>
public static class IHttpClientBuilderExtensions
{
    /// <summary>
    /// The lazily-initialized User Agent to use for all requests. This field is read-only.
    /// </summary>
    private static readonly ProductInfoHeaderValue _userAgent = CreateUserAgent();

    /// <summary>
    /// Applies the default configuration to the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to apply the default configuration to.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/> passed as the value of <paramref name="builder"/>.
    /// </returns>
    public static IHttpClientBuilder ApplyDefaultConfiguration(this IHttpClientBuilder builder)
    {
        builder.Services.AddTransient<CorrelationIdHandler>();

        var httpClientBuilder = builder
            .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHttpHandler)
            .ConfigureHttpClient(ApplyDefaultConfiguration)
            .AddHttpMessageHandler<CorrelationIdHandler>();

        httpClientBuilder.AddStandardResilienceHandler();

        return httpClientBuilder;
    }

    /// <summary>
    /// Applies the configuration for remote authentication to the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to apply the configuration to.</param>
    /// <returns>
    /// The <see cref="IHttpClientBuilder"/> passed as the value of <paramref name="builder"/>.
    /// </returns>
    public static IHttpClientBuilder ApplyRemoteAuthenticationConfiguration(this IHttpClientBuilder builder)
        => builder.ConfigureHttpClient(ApplyRemoteAuthenticationConfiguration);

    /// <summary>
    /// Applies the default configuration to <see cref="HttpClient"/> instances.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
    private static void ApplyDefaultConfiguration(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
        client.Timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(20);
    }

    /// <summary>
    /// Applies the configuration for remote authentication to <see cref="HttpClient"/> instances.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
    private static void ApplyRemoteAuthenticationConfiguration(HttpClient client)
    {
        // Apply configuration settings to HttpClient that would be applied if
        // the authentication libraries created their own HttpClient instances.
        // See https://github.com/aspnet/Security/blob/b3e4bf382c9676e2d912636b7ee0255cad244e11/src/Microsoft.AspNetCore.Authentication.OAuth/OAuthPostConfigureOptions.cs#L34
        // See https://github.com/aspnet/Security/blob/b3e4bf382c9676e2d912636b7ee0255cad244e11/src/Microsoft.AspNetCore.Authentication.Twitter/TwitterPostConfigureOptions.cs#L44-L47
        client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
        client.DefaultRequestHeaders.ExpectContinue = false;
        client.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
    }

    /// <summary>
    /// Creates the primary HTTP message handler to use for all requests.
    /// </summary>
    /// <returns>
    /// The <see cref="HttpMessageHandler"/> to use as the primary message handler.
    /// </returns>
    private static HttpMessageHandler CreatePrimaryHttpHandler()
    {
        return new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
        };
    }

    /// <summary>
    /// Creates the User Agent HTTP request header to use for all requests.
    /// </summary>
    /// <returns>
    /// The <see cref="ProductInfoHeaderValue"/> to use.
    /// </returns>
    private static ProductInfoHeaderValue CreateUserAgent()
    {
        string productVersion = typeof(SiteRoutes)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion!;

        // Truncate the Git commit SHA to 7 characters, if present
        int indexOfPlus = productVersion.IndexOf('+', StringComparison.Ordinal);

        if (indexOfPlus > -1 && indexOfPlus < productVersion.Length - 1)
        {
            string hash = productVersion[(indexOfPlus + 1)..];

            if (hash.Length > 7)
            {
                productVersion = productVersion[..(indexOfPlus + 8)];
            }
        }

        return new ProductInfoHeaderValue("MartinCostello.LondonTravel", productVersion);
    }

    private sealed class CorrelationIdHandler(IHttpContextAccessor accessor) : DelegatingHandler()
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (accessor.HttpContext != null)
            {
                request.Headers.TryAddWithoutValidation("x-correlation-id", accessor.HttpContext.TraceIdentifier);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
