// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using MartinCostello.LondonTravel.Site.Services.Tfl;
    using Microsoft.Extensions.DependencyInjection;
    using Options;
    using Refit;

    /// <summary>
    /// A class containing HTTP-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class HttpServiceCollectionExtensions
    {
        /// <summary>
        /// The lazily-initialized User Agent to use for all requests. This field is read-only.
        /// </summary>
        private static readonly Lazy<ProductInfoHeaderValue> _userAgent = new Lazy<ProductInfoHeaderValue>(CreateUserAgent);

        /// <summary>
        /// Adds HTTP clients to the services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>
        /// The value specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddHttpClients(this IServiceCollection services)
        {
            services
                .AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                .ConfigureHttpClient(ApplyDefaultConfiguration);

            services
                .AddHttpClient<ITflClient>()
                .AddTypedClient(AddTfl)
                .ConfigureHttpClient(ApplyDefaultConfiguration);

            return services;
        }

        /// <summary>
        /// Adds a typed client for the TfL API.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure the client with.</param>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The <see cref="ITflClient"/> to use.
        /// </returns>
        private static ITflClient AddTfl(HttpClient client, IServiceProvider provider)
        {
            var options = provider.GetRequiredService<TflOptions>();

            client.BaseAddress = options.BaseUri;

            return RestService.For<ITflClient>(client);
        }

        /// <summary>
        /// Applies the default configuration to <see cref="HttpClient"/> instances.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to configure.</param>
        private static void ApplyDefaultConfiguration(HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.Add(_userAgent.Value);
            client.Timeout = TimeSpan.FromSeconds(20);
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
