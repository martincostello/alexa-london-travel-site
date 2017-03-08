// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class representing an implementation of <see cref="IDocumentClient"/>. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentClientWrapper : IDocumentClient, IDisposable
    {
        /// <summary>
        /// The <see cref="DocumentClient"/> being wrapped. This field is read-only.
        /// </summary>
        private readonly DocumentClient _client;

        /// <summary>
        /// The logger to use. This field is read-only.
        /// </summary>
        private readonly ILogger<DocumentClientWrapper> _logger;

        /// <summary>
        /// The name of the Azure DocumentDb database. This field is read-only.
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// The name of the Azure DocumentDb collection in the database. This field is read-only.
        /// </summary>
        private readonly string _collectionName;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentClientWrapper"/> class.
        /// </summary>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger{DocumentClientWrapper}"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        public DocumentClientWrapper(UserStoreOptions options, ILogger<DocumentClientWrapper> logger)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ServiceUri == null)
            {
                throw new ArgumentException("No DocumentDb URI is configured.", nameof(options));
            }

            if (!options.ServiceUri.IsAbsoluteUri)
            {
                throw new ArgumentException("The configured DocumentDb URI is as it is not an absolute URI.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.AccessKey))
            {
                throw new ArgumentException("No DocumentDb access key is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.DatabaseName))
            {
                throw new ArgumentException("No DocumentDb database name is configured.", nameof(options));
            }

            if (string.IsNullOrEmpty(options.CollectionName))
            {
                throw new ArgumentException("No DocumentDb collection name is configured.", nameof(options));
            }

            _client = new DocumentClient(options.ServiceUri, options.AccessKey);
            _logger = logger;
            _databaseName = options.DatabaseName;
            _collectionName = options.CollectionName;
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
        public async Task<string> CreateAsync(object document)
        {
            await EnsureCollectionExistsAsync();

            _logger?.LogInformation($"Creating document in collection '{_collectionName}' of database '{_databaseName}'.");

            var result = await _client.CreateDocumentAsync(BuildCollectionUri(), document);

            _logger?.LogInformation($"Created document in collection '{_collectionName}' of database '{_databaseName}'. Id: '{result.Resource.Id}'.");

            return result.Resource.Id;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            await EnsureCollectionExistsAsync();

            try
            {
                await _client.DeleteDocumentAsync(BuildDocumentUri(id));
                return true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to delete document with Id '{id}'.");
                    throw;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string id)
            where T : class
        {
            await EnsureCollectionExistsAsync();
            T result = null;

            try
            {
                Document document = await _client.ReadDocumentAsync(BuildDocumentUri(id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to query document with Id '{id}'.");
                    throw;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
            where T : class
        {
            await EnsureCollectionExistsAsync();

            var documents = new List<T>();

            _logger?.LogTrace($"Querying documents in collection '{_collectionName}' of database '{_databaseName}'.");

            using (var query = _client.CreateDocumentQuery<T>(BuildCollectionUri()).Where(predicate).AsDocumentQuery())
            {
                while (query.HasMoreResults)
                {
                    documents.AddRange(await query.ExecuteNextAsync<T>(cancellationToken));
                }
            }

            _logger?.LogTrace($"Found {documents.Count:N0} document(s) in collection '{_collectionName}' of database '{_databaseName}' that matched query.");

            return documents;
        }

        /// <inheritdoc />
        public async Task ReplaceAsync(string id, object document)
        {
            await EnsureCollectionExistsAsync();

            _logger?.LogInformation($"Replacing document with Id '{id}' in collection '{_collectionName}' of database '{_databaseName}'.");

            await _client.ReplaceDocumentAsync(BuildDocumentUri(id), document);

            _logger?.LogInformation($"Replaced document wuth Id '{id}' in collection '{_collectionName}' of database '{_databaseName}'.");
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
                if (ex.StatusCode != System.Net.HttpStatusCode.NotFound)
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
        /// Ensures that the collection exists as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the collection exists.
        /// </returns>
        private async Task EnsureCollectionExistsAsync()
        {
            await EnsureDatabaseExistsAsync();

            bool collectionExists;

            try
            {
                await _client.ReadDocumentCollectionAsync(BuildCollectionUri());
                collectionExists = true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to read collection '{_collectionName}' in database '{_databaseName}'.");
                    throw;
                }

                _logger?.LogInformation($"Collection '{_collectionName}' does not exist in database '{_databaseName}'.");
                collectionExists = false;
            }

            if (!collectionExists)
            {
                try
                {
                    _logger?.LogInformation($"Creating collection '{_collectionName}' in database '{_databaseName}'.");

                    await _client.CreateDocumentCollectionIfNotExistsAsync(
                        BuildDatabaseUri(),
                        new DocumentCollection() { Id = _collectionName },
                        new RequestOptions() { OfferThroughput = 400 });

                    _logger?.LogInformation($"Created collection '{_collectionName}' in database '{_databaseName}'.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(default(EventId), ex, $"Failed to create collection '{_collectionName}' in database '{_databaseName}'.");
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
        /// <returns>
        /// The URI to use for the collection.
        /// </returns>
        private Uri BuildCollectionUri() => UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);

        /// <summary>
        /// Builds a URI for the specified document Id.
        /// </summary>
        /// <param name="id">The Id of the document.</param>
        /// <returns>
        /// The URI to use for the specified document.
        /// </returns>
        private Uri BuildDocumentUri(string id) => UriFactory.CreateDocumentUri(_databaseName, _collectionName, id);
    }
}
