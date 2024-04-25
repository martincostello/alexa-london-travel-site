// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace MartinCostello.LondonTravel.Site.Services.Data;

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
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var options = serviceProvider.GetRequiredService<UserStoreOptions>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        return CreateClient(configuration["ConnectionStrings:Cosmos"]!, options, httpClientFactory);
    }

    /// <summary>
    /// Creates a new instance of an <see cref="IDocumentClient"/> implementation.
    /// </summary>
    /// <param name="connectionString">The Azire Cosmos DB connection string to use.</param>
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
        string connectionString,
        UserStoreOptions options,
        IHttpClientFactory httpClientFactory,
        CosmosSerializer? serializer = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (string.IsNullOrEmpty(options.DatabaseName))
        {
            throw new ArgumentException("No DocumentDB database name is configured.", nameof(options));
        }

        if (string.IsNullOrEmpty(options.CollectionName))
        {
            throw new ArgumentException("No DocumentDB collection name is configured.", nameof(options));
        }

        var builder = new CosmosClientBuilder(connectionString)
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
