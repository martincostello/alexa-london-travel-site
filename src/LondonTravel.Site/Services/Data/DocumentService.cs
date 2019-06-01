// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Microsoft.Extensions.Logging;
    using Options;

    /// <summary>
    /// A class representing an implementation of <see cref="IDocumentService"/>. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentService : IDocumentService
    {
        /// <summary>
        /// The <see cref="IDocumentClient"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentClient _client;

        /// <summary>
        /// The <see cref="IDocumentCollectionInitializer"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentCollectionInitializer _initializer;

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
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentService"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
        /// <param name="initializer">The <see cref="IDocumentCollectionInitializer"/> to use.</param>
        /// <param name="telemetry">The <see cref="TelemetryClient"/> to use.</param>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger{DocumentService}"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="client"/> or <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        public DocumentService(
            IDocumentClient client,
            IDocumentCollectionInitializer initializer,
            TelemetryClient telemetry,
            UserStoreOptions options,
            ILogger<DocumentService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));

            _telemetry = telemetry;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(object document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            await EnsureCollectionExistsAsync();

            _logger.LogTrace(
                "Creating document in collection {CollectionName} of database {DatabaseName}.",
                _options.CollectionName,
                _options.DatabaseName);

            Uri uri = BuildCollectionUri();

            var result = await _client.CreateDocumentAsync(uri, document);

            _logger.LogTrace(
                "Created document in collection {CollectionName} of database {DatabaseName}. Id: {ResourceId}.",
                _options.CollectionName,
                _options.DatabaseName,
                result.Resource.Id);

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

                await _client.DeleteDocumentAsync(uri);

                return true;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
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
            T result;

            try
            {
                Uri uri = BuildDocumentUri(id);

                Document document = await _client.ReadDocumentAsync(uri);

                result = (T)(dynamic)document;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                result = null;
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
            where T : class
        {
            await EnsureCollectionExistsAsync();

            var documents = new List<T>();

            _logger.LogTrace(
                "Querying documents in collection {CollectionName} of database {DatabaseName}.",
                _options.CollectionName,
                _options.DatabaseName);

            Uri uri = BuildCollectionUri();
            Uri queryUri = new Uri($"{uri}/{DocumentHelpers.DocumentsUriFragment}", UriKind.Relative);

            using (var query = _client.CreateDocumentQuery<T>(uri).Where(predicate).AsDocumentQuery())
            {
                while (query.HasMoreResults)
                {
                    documents.AddRange(await query.ExecuteNextAsync<T>(cancellationToken));
                }
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
            var uri = BuildCollectionUri();
            var collection = await _client.ReadDocumentCollectionAsync(uri);

            return GetDocumentCount(collection.CurrentResourceQuotaUsage);
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

            _logger.LogTrace(
                "Replacing document with Id {Id} in collection {CollectionName} of database {DatabaseName}.",
                id,
                _options.CollectionName,
                _options.DatabaseName);

            RequestOptions options = GetOptionsForETag(etag);

            try
            {
                Uri uri = BuildDocumentUri(id);

                Document response = await _client.ReplaceDocumentAsync(uri, document, options);

                _logger.LogTrace(
                    "Replaced document with Id {Id} in collection {CollectionName} of database {DatabaseName}.",
                    id,
                    _options.CollectionName,
                    _options.DatabaseName);

                return (T)(dynamic)response;
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
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
        /// Returns the document count from the specified current resource quota usage string.
        /// </summary>
        /// <param name="currentResourceQuotaUsage">A string containing the current resource quota usage.</param>
        /// <returns>
        /// The current document count, if successfully determined; otherwise -1.
        /// </returns>
        private static long GetDocumentCount(string currentResourceQuotaUsage)
        {
            long count = -1;

            const string Key = "documentsCount=";
            int indexOfCountKey = currentResourceQuotaUsage?.IndexOf(Key, StringComparison.Ordinal) ?? -1;

            if (indexOfCountKey > -1)
            {
                int searchFromIndex = indexOfCountKey + Key.Length;
                int endOfCountIndex = currentResourceQuotaUsage.IndexOf(";", searchFromIndex, StringComparison.Ordinal);

                if (endOfCountIndex > -1)
                {
                    string countAsString = currentResourceQuotaUsage.Substring(
                        searchFromIndex,
                        endOfCountIndex - searchFromIndex);

                    if (!long.TryParse(countAsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
                    {
                        count = -1;
                    }
                }
            }

            return count;
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
                Type = AccessConditionType.IfMatch,
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
        private Task EnsureCollectionExistsAsync()
            => _initializer.EnsureCollectionExistsAsync(_client, _options.CollectionName);

        /// <summary>
        /// Builds a URI for the collection.
        /// </summary>
        /// <returns>
        /// The URI to use for the collection.
        /// </returns>
        private Uri BuildCollectionUri()
            => UriFactory.CreateDocumentCollectionUri(_options.DatabaseName, _options.CollectionName);

        /// <summary>
        /// Builds a URI for the specified document Id.
        /// </summary>
        /// <param name="id">The Id of the document.</param>
        /// <returns>
        /// The URI to use for the specified document.
        /// </returns>
        private Uri BuildDocumentUri(string id)
            => UriFactory.CreateDocumentUri(_options.DatabaseName, _options.CollectionName, id);
    }
}
