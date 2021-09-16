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
public sealed class DocumentService : IDocumentService
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

        Container container = await GetContainerAsync();

        _logger.LogTrace(
            "Creating document in collection {CollectionName} of database {DatabaseName}.",
            _options.CollectionName,
            _options.DatabaseName);

        document.Id = Guid.NewGuid().ToString();

        var result = await container.CreateItemAsync(document);

        _logger.LogTrace(
            "Created document in collection {CollectionName} of database {DatabaseName}. Id: {ResourceId}.",
            _options.CollectionName,
            _options.DatabaseName,
            result.Resource.Id);

        return result.Resource.Id!;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        Container container = await GetContainerAsync();

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

        Container container = await GetContainerAsync();

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
        Container container = await GetContainerAsync();

        _logger.LogTrace(
            "Querying documents in collection {CollectionName} of database {DatabaseName}.",
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

        _logger.LogTrace(
            "Found {DocumentCount:N0} document(s) in collection {CollectionName} of database {DatabaseName} that matched query.",
            documents.Count,
            _options.CollectionName,
            _options.DatabaseName);

        return documents;
    }

    /// <inheritdoc />
    public async Task<long> GetDocumentCountAsync()
    {
        Container container = await GetContainerAsync();

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

        Container container = await GetContainerAsync();

        string? id = document.Id;

        _logger.LogTrace(
            "Replacing document with Id {Id} in collection {CollectionName} of database {DatabaseName}.",
            id,
            _options.CollectionName,
            _options.DatabaseName);

        ItemRequestOptions? requestOptions = GetOptionsForETag(etag);

        try
        {
            LondonTravelUser updated = await container.ReplaceItemAsync(document, id, requestOptions: requestOptions);

            _logger.LogTrace(
                "Replaced document with Id {Id} in collection {CollectionName} of database {DatabaseName}.",
                id,
                _options.CollectionName,
                _options.DatabaseName);

            return updated;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            _logger.LogWarning(
                "Failed to replace document with Id {Id} in collection {CollectionName} of database {DatabaseName} as the write would conflict. ETag: {ETag}.",
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
}
