// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Net;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Azure.Cosmos;

namespace MartinCostello.LondonTravel.Site.Services.Data;

/// <summary>
/// A class representing the default implementation of
/// <see cref="IDocumentCollectionInitializer"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DocumentCollectionInitializer"/> class.
/// </remarks>
/// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
/// <param name="logger">The <see cref="ILogger{DocumentCollectionInitializer}"/> to use.</param>
/// <exception cref="ArgumentNullException">
/// <paramref name="options"/> is <see langword="null"/>.
/// </exception>
/// <exception cref="ArgumentException">
/// <paramref name="options"/> is invalid.
/// </exception>
public sealed partial class DocumentCollectionInitializer(
    UserStoreOptions options,
    ILogger<DocumentCollectionInitializer> logger) : IDocumentCollectionInitializer
{
    /// <summary>
    /// The name of the Azure DocumentDB database. This field is read-only.
    /// </summary>
    private readonly string? _databaseName = options.DatabaseName;

    /// <summary>
    /// The containers that have been checked to exist. This field is read-only.
    /// </summary>
    private readonly ConcurrentDictionary<string, bool> _existingContainers = new();

    /// <inheritdoc />
    public async Task<bool> EnsureCollectionExistsAsync(
        CosmosClient client,
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(collectionName);

        if (_existingContainers.ContainsKey(collectionName))
        {
            return true;
        }

        var database = await GetOrCreateDatabaseAsync(client, cancellationToken);

        return await CreateContainerAsync(collectionName, database, cancellationToken);
    }

    /// <summary>
    /// Gets or creates the database as an asynchronous operation.
    /// </summary>
    /// <param name="client">The Cosmos client to use to get the database.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>
    /// A <see cref="Task{Database}"/> representing the asynchronous operation to get the database.
    /// </returns>
    private async Task<Database> GetOrCreateDatabaseAsync(CosmosClient client, CancellationToken cancellationToken)
    {
        var response = await client.CreateDatabaseIfNotExistsAsync(
            _databaseName,
            cancellationToken: cancellationToken);

        bool created = response.StatusCode == HttpStatusCode.Created;

        if (created)
        {
            Log.CreatedDatabase(logger, _databaseName);
        }

        return response.Database;
    }

    /// <summary>
    /// Creates the container as an asynchronous operation.
    /// </summary>
    /// <param name="id">The container Id.</param>
    /// <param name="database">The database to create the container in.</param>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>
    /// A <see cref="Task{Database}"/> representing the asynchronous operation to create the container.
    /// </returns>
    private async Task<bool> CreateContainerAsync(
        string id,
        Database database,
        CancellationToken cancellationToken)
    {
        var containerProperties = new ContainerProperties(id, "/_partitionKey");

        var response = await database.CreateContainerIfNotExistsAsync(
            containerProperties,
            throughput: 400,
            cancellationToken: cancellationToken);

        bool created = response.StatusCode == HttpStatusCode.Created;

        if (created)
        {
            Log.CreatedCollection(logger, id, _databaseName);
        }

        return created;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "Created database {DatabaseName}.")]
        public static partial void CreatedDatabase(ILogger logger, string? databaseName);

        [LoggerMessage(
           EventId = 2,
           Level = LogLevel.Information,
           Message = "Created collection {CollectionName} in database {DatabaseName}.")]
        public static partial void CreatedCollection(ILogger logger, string collectionName, string? databaseName);
    }
}
