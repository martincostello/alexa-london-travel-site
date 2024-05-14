// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using System.Net;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace MartinCostello.LondonTravel.Site.Services.Data;

/// <summary>
/// A class representing an implementation of <see cref="IDocumentService"/>. This class cannot be inherited.
/// </summary>
public sealed partial class DocumentService : IDocumentService
{
    /// <summary>
    /// The <see cref="CosmosClient"/> to use. This field is read-only.
    /// </summary>
    private readonly CosmosClient _client;

    /// <summary>
    /// The <see cref="IDocumentCollectionInitializer"/> to use. This field is read-only.
    /// </summary>
    private readonly IDocumentCollectionInitializer _initializer;

    /// <summary>
    /// The <see cref="UserStoreOptions"/> to use. This field is read-only.
    /// </summary>
    private readonly UserStoreOptions _options;

    /// <summary>
    /// The logger to use. This field is read-only.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentService"/> class.
    /// </summary>
    /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
    /// <param name="initializer">The <see cref="IDocumentCollectionInitializer"/> to use.</param>
    /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger{DocumentService}"/> to use.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="client"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="options"/> is invalid.
    /// </exception>
    public DocumentService(
        CosmosClient client,
        IDocumentCollectionInitializer initializer,
        UserStoreOptions options,
        ILogger<DocumentService> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(initializer);

        _client = client;
        _initializer = initializer;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> CreateAsync(LondonTravelUser document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var container = await GetContainerAsync();

        Log.CreatingDocument(
            _logger,
            _options.CollectionName,
            _options.DatabaseName);

        document.Id = Guid.NewGuid().ToString();

        var result = await container.CreateItemAsync(document);

        Log.CreatedDocument(
            _logger,
            _options.CollectionName,
            _options.DatabaseName,
            result.Resource.Id);

        return result.Resource.Id!;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var container = await GetContainerAsync();

        try
        {
            var result = await container.DeleteItemAsync<LondonTravelUser>(id, PartitionKey.None);

            return result.StatusCode == HttpStatusCode.NoContent;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> GetAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        var container = await GetContainerAsync();

        try
        {
            return await container.ReadItemAsync<LondonTravelUser>(id, PartitionKey.None);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LondonTravelUser>> GetAsync(
        Expression<Func<LondonTravelUser, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var container = await GetContainerAsync();

        Log.QueryingDocuments(
            _logger,
            _options.CollectionName,
            _options.DatabaseName);

        var queryable = container.GetItemLinqQueryable<LondonTravelUser>();
        var iterator = queryable.Where(predicate).ToFeedIterator();

        var documents = new List<LondonTravelUser>();

        while (iterator.HasMoreResults)
        {
            var items = await iterator.ReadNextAsync(cancellationToken);
            documents.AddRange(items.Resource);
        }

        Log.QueriedDocuments(
            _logger,
            documents.Count,
            _options.CollectionName,
            _options.DatabaseName);

        return documents;
    }

    /// <inheritdoc />
    public async Task<long> GetDocumentCountAsync()
    {
        var container = await GetContainerAsync();

        var iterator = container.GetItemQueryIterator<int>("SELECT VALUE COUNT(1) FROM c");

        if (!iterator.HasMoreResults)
        {
            return 0;
        }

        var response = await iterator.ReadNextAsync();

        return response.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<LondonTravelUser?> ReplaceAsync(LondonTravelUser document, string? etag)
    {
        ArgumentNullException.ThrowIfNull(document);

        var container = await GetContainerAsync();

        string? id = document.Id;

        Log.ReplacingDocument(
            _logger,
            id,
            _options.CollectionName,
            _options.DatabaseName);

        var requestOptions = GetOptionsForETag(etag);

        try
        {
            LondonTravelUser updated = await container.ReplaceItemAsync(document, id, requestOptions: requestOptions);

            Log.ReplacedDocument(
                _logger,
                id,
                _options.CollectionName,
                _options.DatabaseName);

            return updated;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            Log.ReplaceFailedWithConflict(
                _logger,
                id,
                _options.CollectionName,
                _options.DatabaseName,
                etag);
        }

        return null;
    }

    /// <summary>
    /// Gets the <see cref="ItemRequestOptions"/> to use for the specified ETag value.
    /// </summary>
    /// <param name="etag">The value of the ETag.</param>
    /// <returns>
    /// The created instance of <see cref="ItemRequestOptions"/>, if any.
    /// </returns>
    private static ItemRequestOptions? GetOptionsForETag(string? etag)
    {
        if (etag == null)
        {
            return null;
        }

        return new ItemRequestOptions()
        {
            IfMatchEtag = etag,
        };
    }

    /// <summary>
    /// Gets the document container as an asynchronous operation.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the container.
    /// </returns>
    private async Task<Container> GetContainerAsync()
    {
        await _initializer.EnsureCollectionExistsAsync(_client, _options.CollectionName!);

        return _client.GetContainer(_options.DatabaseName, _options.CollectionName);
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Trace,
            Message = "Creating document in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void CreatingDocument(ILogger logger, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Trace,
            Message = "Created document in collection {CollectionName} of database {DatabaseName}. Id: {ResourceId}.")]
        public static partial void CreatedDocument(ILogger logger, string? collectionName, string? databaseName, string? resourceId);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Trace,
            Message = "Querying documents in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void QueryingDocuments(ILogger logger, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Trace,
            Message = "Found {DocumentCount:N0} document(s) in collection {CollectionName} of database {DatabaseName} that matched query.")]
        public static partial void QueriedDocuments(ILogger logger, int documentCount, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Trace,
            Message = "Replacing document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void ReplacingDocument(ILogger logger, string? resourceId, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Trace,
            Message = "Replaced document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void ReplacedDocument(ILogger logger, string? resourceId, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 7,
            Level = LogLevel.Warning,
            Message = "Failed to replace document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName} as the write would conflict. ETag: {ETag}.")]
        public static partial void ReplaceFailedWithConflict(ILogger logger, string? resourceId, string? collectionName, string? databaseName, string? etag);
    }
}
