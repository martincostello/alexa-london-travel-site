// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    /// <summary>
    /// A class containing helper methods for DocumentDB operations. This class cannot be inherited.
    /// </summary>
    internal static class DocumentHelpers
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IDocumentClient"/> implementation.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="IDocumentClient"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceProvider"/> is <see langword="null"/>.
        /// </exception>
        internal static CosmosClient CreateClient(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var options = serviceProvider.GetRequiredService<UserStoreOptions>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            return CreateClient(options, httpClientFactory);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="IDocumentClient"/> implementation.
        /// </summary>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> to use.</param>
        /// <param name="serializer">The optional <see cref="CosmosSerializer"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="IDocumentClient"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> or <paramref name="httpClientFactory"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        internal static CosmosClient CreateClient(
            UserStoreOptions options,
            IHttpClientFactory httpClientFactory,
            CosmosSerializer? serializer = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (httpClientFactory == null)
            {
                throw new ArgumentNullException(nameof(httpClientFactory));
            }

            if (options.ServiceUri == null)
            {
                throw new ArgumentException("No DocumentDB URI is configured.", nameof(options));
            }

            if (!options.ServiceUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The configured DocumentDB URI is as it is not an absolute URI.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.AccessKey))
            {
                throw new ArgumentException("No DocumentDB access key is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.DatabaseName))
            {
                throw new ArgumentException("No DocumentDB database name is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.CollectionName))
            {
                throw new ArgumentException("No DocumentDB collection name is configured.", nameof(options));
            }

            var builder = new CosmosClientBuilder(options.ServiceUri.AbsoluteUri, options.AccessKey)
                .WithApplicationName("london-travel")
                .WithCustomSerializer(serializer)
                .WithHttpClientFactory(httpClientFactory.CreateClient)
                .WithRequestTimeout(TimeSpan.FromSeconds(15));

            if (!string.IsNullOrEmpty(options.CurrentLocation))
            {
                builder = builder.WithApplicationRegion(options.CurrentLocation);
            }

            return builder.Build();
        }
    }
}
