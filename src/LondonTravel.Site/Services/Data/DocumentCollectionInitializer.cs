// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.Logging;
    using Options;

    /// <summary>
    /// A class representing the default implementation of
    /// <see cref="IDocumentCollectionInitializer"/>. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentCollectionInitializer : IDocumentCollectionInitializer
    {
        /// <summary>
        /// The <see cref="DocumentClient"/> being wrapped. This field is read-only.
        /// </summary>
        private readonly DocumentClient _client;

        /// <summary>
        /// The logger to use. This field is read-only.
        /// </summary>
        private readonly ILogger<DocumentCollectionInitializer> _logger;

        /// <summary>
        /// The name of the Azure DocumentDb database. This field is read-only.
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// The collections that have been checked to exist. This field is read-only.
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _existingCollections;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentCollectionInitializer"/> class.
        /// </summary>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger{DocumentCollectionInitializer}"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        public DocumentCollectionInitializer(UserStoreOptions options, ILogger<DocumentCollectionInitializer> logger)
        {
            _client = DocumentHelpers.CreateClient(options);
            _existingCollections = new ConcurrentDictionary<string, bool>();

            _logger = logger;
            _databaseName = options.DatabaseName;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> EnsureCollectionExistsAsync(string collectionName)
        {
            if (collectionName == null)
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            if (_existingCollections.ContainsKey(collectionName))
            {
                return true;
            }

            await EnsureDatabaseExistsAsync();

            bool collectionExists;

            try
            {
                await _client.ReadDocumentCollectionAsync(BuildCollectionUri(collectionName));
                collectionExists = true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to read collection '{collectionName}' in database '{_databaseName}'.");
                    throw;
                }

                _logger?.LogInformation($"Collection '{collectionName}' does not exist in database '{_databaseName}'.");
                collectionExists = false;
            }

            bool created = false;

            if (!collectionExists)
            {
                try
                {
                    _logger?.LogInformation($"Creating collection '{collectionName}' in database '{_databaseName}'.");

                    var response = await _client.CreateDocumentCollectionIfNotExistsAsync(
                        BuildDatabaseUri(),
                        new DocumentCollection() { Id = collectionName },
                        new RequestOptions() { OfferThroughput = 400 });

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        created = true;
                    }

                    _logger?.LogInformation($"Created collection '{collectionName}' in database '{_databaseName}'.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to create collection '{collectionName}' in database '{_databaseName}'.");
                    throw;
                }
            }

            _existingCollections.AddOrUpdate(collectionName, created, (p, r) => true);

            return created;
        }

        /// <summary>
        /// Ensures that the database exists as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the database exists.
        /// </returns>
        private async Task EnsureDatabaseExistsAsync()
        {
            bool databaseExists;

            try
            {
                await _client.ReadDatabaseAsync(BuildDatabaseUri());
                databaseExists = true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to read database '{_databaseName}'.");
                    throw;
                }

                _logger?.LogInformation($"Database '{_databaseName}' does not exist.");
                databaseExists = false;
            }

            if (!databaseExists)
            {
                try
                {
                    _logger?.LogInformation($"Creating database '{_databaseName}'.");

                    await _client.CreateDatabaseIfNotExistsAsync(new Database() { Id = _databaseName });

                    _logger?.LogInformation($"Created database '{_databaseName}'.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to create database '{_databaseName}'.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Builds a URI for the database.
        /// </summary>
        /// <returns>
        /// The URI to use for the database.
        /// </returns>
        private Uri BuildDatabaseUri() => UriFactory.CreateDatabaseUri(_databaseName);

        /// <summary>
        /// Builds a URI for the collection.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>
        /// The URI to use for the collection.
        /// </returns>
        private Uri BuildCollectionUri(string collectionName) => UriFactory.CreateDocumentCollectionUri(_databaseName, collectionName);
    }
}
