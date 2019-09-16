// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Options;

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

            var jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>().Value;
            var serializer = new SystemTextJsonCosmosSerializer(jsonOptions.JsonSerializerOptions);

            return CreateClient(options, serializer);
        }

        /// <summary>
        /// Creates a new instance of an <see cref="IDocumentClient"/> implementation.
        /// </summary>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="serializer">The optional <see cref="CosmosSerializer"/> to use.</param>
        /// <returns>
        /// The created instance of <see cref="IDocumentClient"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        internal static CosmosClient CreateClient(UserStoreOptions options, CosmosSerializer? serializer = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
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

            var cosmosOptions = new CosmosClientOptions()
            {
                ApplicationName = "london-travel",
                RequestTimeout = TimeSpan.FromSeconds(15),
                Serializer = serializer,
            };

            if (!string.IsNullOrEmpty(options.CurrentLocation))
            {
                cosmosOptions.ApplicationRegion = options.CurrentLocation;
            }

            return new CosmosClient(options.ServiceUri.AbsoluteUri, options.AccessKey, cosmosOptions);
        }
    }
}
