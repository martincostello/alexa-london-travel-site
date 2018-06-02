// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;
    using Polly.Extensions.Http;

    /// <summary>
    /// A class containing extension methods for the <see cref="IHttpClientBuilder"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IHttpClientBuilderExtensions
    {
        /// <summary>
        /// The lazily-initialized User Agent to use for all requests. This field is read-only.
        /// </summary>
        private static readonly Lazy<ProductInfoHeaderValue> _userAgent = new Lazy<ProductInfoHeaderValue>(CreateUserAgent);

        /// <summary>
        /// Applies the default configuration to the <see cref="IHttpClientBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/> to apply the default configuration to.</param>
        /// <returns>
        /// The <see cref="IHttpClientBuilder"/> passed as the value of <paramref name="builder"/>.
        /// </returns>
        public static IHttpClientBuilder ApplyDefaultConfiguration(this IHttpClientBuilder builder)
        {
            return builder
                .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHttpHandler)
                .ConfigureHttpClient(ApplyDefaultConfiguration)
                .AddPolicyHandler(CreatePolicyForRequest);
        }

        /// <summary>
        /// Applies the default configuration to <see cref="HttpClient"/> instances.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
        private static void ApplyDefaultConfiguration(HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent.Value);
            client.Timeout = Debugger.IsAttached ? TimeSpan.FromMinutes(1) : TimeSpan.FromSeconds(20);
        }

        /// <summary>
        /// Creates a policy to use for an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request to configure the policy for.</param>
        /// <returns>
        /// The policy to use.
        /// </returns>
        private static IAsyncPolicy<HttpResponseMessage> CreatePolicyForRequest(HttpRequestMessage request)
        {
            var sleepDurations = new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
            };

            var readPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(sleepDurations)
                .WithPolicyKey("ReadPolicy");

            var writePolicy = Policy.NoOpAsync()
                .AsAsyncPolicy<HttpResponseMessage>()
                .WithPolicyKey("WritePolicy");

            return request.Method == HttpMethod.Get ? readPolicy : writePolicy;
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
            string productVersion = typeof(Startup)
                .GetTypeInfo()
                .Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            // Truncate the Git commit SHA to 7 characters, if present
            int indexOfPlus = productVersion.IndexOf('+', StringComparison.Ordinal);

            if (indexOfPlus > -1 && indexOfPlus < productVersion.Length - 1)
            {
                string hash = productVersion.Substring(indexOfPlus + 1);

                if (hash.Length > 7)
                {
                    productVersion = productVersion.Substring(0, indexOfPlus + 8);
                }
            }

            return new ProductInfoHeaderValue("MartinCostello.LondonTravel", productVersion);
        }
    }
}
