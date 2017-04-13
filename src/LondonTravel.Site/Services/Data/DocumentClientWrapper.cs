// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.Logging;
    using Options;

    /// <summary>
    /// A class representing an implementation of <see cref="IDocumentClient"/>. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentClientWrapper : IDocumentClient, IDisposable
    {
        /// <summary>
        /// The <see cref="IDocumentCollectionInitializer"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentCollectionInitializer _initializer;

        /// <summary>
        /// The <see cref="DocumentClient"/> being wrapped. This field is read-only.
        /// </summary>
        private readonly DocumentClient _client;

        /// <summary>
        /// The <see cref="TelemetryClient"/> to use. This field is read-only.
        /// </summary>
        private readonly TelemetryClient _telemetry;

        /// <summary>
        /// The <see cref="UserStoreOptions"/> to use. This field is read-only.
        /// </summary>
        private readonly UserStoreOptions _options;

        /// <summary>
        /// The logger to use. This field is read-only.
        /// </summary>
        private readonly ILogger<DocumentClientWrapper> _logger;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentClientWrapper"/> class.
        /// </summary>
        /// <param name="initializer">The <see cref="IDocumentCollectionInitializer"/> to use.</param>
        /// <param name="telemetry">The <see cref="TelemetryClient"/> to use.</param>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger{DocumentClientWrapper}"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        public DocumentClientWrapper(
            IDocumentCollectionInitializer initializer,
            TelemetryClient telemetry,
            UserStoreOptions options,
            ILogger<DocumentClientWrapper> logger)
        {
            _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            _client = DocumentHelpers.CreateClient(options);

            _telemetry = telemetry;
            _options = options;
            _logger = logger;
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
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            await EnsureCollectionExistsAsync();

            _logger.LogTrace($"Creating document in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'.");

            Uri uri = BuildCollectionUri();

            var result = await TrackAsync(HttpMethod.Post, uri, () => _client.CreateDocumentAsync(uri, document));

            _logger.LogTrace($"Created document in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'. Id: '{result.Resource.Id}'.");

            return result.Resource.Id;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            await EnsureCollectionExistsAsync();

            try
            {
                Uri uri = BuildDocumentUri(id);

                await TrackAsync(HttpMethod.Delete, uri, () => _client.DeleteDocumentAsync(uri));

                return true;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError(default(EventId), ex, $"Failed to delete document with Id '{id}'.");
                    throw;
                }

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string id)
            where T : class
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            await EnsureCollectionExistsAsync();
            T result = null;

            try
            {
                Uri uri = BuildDocumentUri(id);

                Document document = await TrackAsync(HttpMethod.Get, uri, () => _client.ReadDocumentAsync(uri));

                result = (T)(dynamic)document;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                {
                    _logger.LogError(default(EventId), ex, $"Failed to query document with Id '{id}'.");
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

            _logger.LogTrace($"Querying documents in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'.");

            Uri uri = BuildCollectionUri();
            Uri queryUri = new Uri($"{uri}/{DocumentHelpers.DocumentsUriFragment}", UriKind.Relative);

            using (var query = _client.CreateDocumentQuery<T>(uri).Where(predicate).AsDocumentQuery())
            {
                while (query.HasMoreResults)
                {
                    documents.AddRange(await TrackQueryAsync(queryUri, () => query.ExecuteNextAsync<T>(cancellationToken)));
                }
            }

            _logger.LogTrace($"Found {documents.Count:N0} document(s) in collection '{_options.CollectionName}' of database '{_options.DatabaseName}' that matched query.");

            return documents;
        }

        /// <inheritdoc />
        public async Task<T> ReplaceAsync<T>(string id, T document, string etag)
            where T : class
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            await EnsureCollectionExistsAsync();

            _logger.LogTrace($"Replacing document with Id '{id}' in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'.");

            RequestOptions options = GetOptionsForETag(etag);

            try
            {
                Uri uri = BuildDocumentUri(id);

                Document response = await TrackAsync(HttpMethod.Put, uri, () => _client.ReplaceDocumentAsync(uri, document, options));

                _logger.LogTrace($"Replaced document with Id '{id}' in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'.");

                return (T)(dynamic)response;
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode != HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogError(default(EventId), ex, $"Failed to replace document with Id '{id}' in collection '{_options.CollectionName}' of database '{_options.DatabaseName}'.");
                    throw;
                }

                _logger.LogWarning($"Failed to replace document with Id '{id}' in collection '{_options.CollectionName}' of database '{_options.DatabaseName}' as the write would conflict. ETag: '{etag}'.");
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="RequestOptions"/> to use for the specified ETag value.
        /// </summary>
        /// <param name="etag">The value of the ETag.</param>
        /// <returns>
        /// The created instance of <see cref="RequestOptions"/>, if any.
        /// </returns>
        private RequestOptions GetOptionsForETag(string etag)
        {
            if (etag == null)
            {
                return null;
            }

            var accessCondition = new AccessCondition()
            {
                Condition = etag,
                Type = AccessConditionType.IfMatch
            };

            return new RequestOptions()
            {
                AccessCondition = accessCondition,
            };
        }

        /// <summary>
        /// Ensures that the collection exists as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the collection exists.
        /// </returns>
        private async Task EnsureCollectionExistsAsync()
        {
            await _initializer.EnsureCollectionExistsAsync(_options.CollectionName);
        }

        /// <summary>
        /// Builds a URI for the collection.
        /// </summary>
        /// <returns>
        /// The URI to use for the collection.
        /// </returns>
        private Uri BuildCollectionUri() => UriFactory.CreateDocumentCollectionUri(_options.DatabaseName, _options.CollectionName);

        /// <summary>
        /// Builds a URI for the specified document Id.
        /// </summary>
        /// <param name="id">The Id of the document.</param>
        /// <returns>
        /// The URI to use for the specified document.
        /// </returns>
        private Uri BuildDocumentUri(string id) => UriFactory.CreateDocumentUri(_options.DatabaseName, _options.CollectionName, id);

        /// <summary>
        /// Tracks the specified request as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the response.</typeparam>
        /// <param name="method">The HTTP method associated with the request.</param>
        /// <param name="relativeUri">The relative URI associated with the request.</param>
        /// <param name="request">A delegate to a method representing the request.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation which
        /// returns an instance of <typeparamref name="T"/> representing the result of the request.
        /// </returns>
        private Task<T> TrackAsync<T>(HttpMethod method, Uri relativeUri, Func<Task<T>> request)
            where T : IResourceResponseBase
        {
            return DocumentHelpers.TrackAsync(_telemetry, _client.ServiceEndpoint, method, relativeUri, request);
        }

        /// <summary>
        /// Tracks the specified query as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T">The type of the query result.</typeparam>
        /// <param name="relativeUri">The relative URI associated with the query.</param>
        /// <param name="request">A delegate to a method representing the query.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation which
        /// returns an feed response of <typeparamref name="T"/> representing the result of the query.
        /// </returns>
        private Task<FeedResponse<T>> TrackQueryAsync<T>(Uri relativeUri, Func<Task<FeedResponse<T>>> request)
        {
            return DocumentHelpers.TrackQueryAsync(_telemetry, _client.ServiceEndpoint, relativeUri, request);
        }
    }
}
