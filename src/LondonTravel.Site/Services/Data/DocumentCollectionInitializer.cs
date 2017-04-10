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
    using Telemetry;

    /// <summary>
    /// A class representing the default implementation of
    /// <see cref="IDocumentCollectionInitializer"/>. This class cannot be inherited.
    /// </summary>
    public sealed class DocumentCollectionInitializer : IDocumentCollectionInitializer
    {
        /// <summary>
        /// The <see cref="DocumentClient"/> being wrapped. This field is read-only.
        /// </summary>
        private readonly DocumentClient _documentClient;

        /// <summary>
        /// The <see cref="ISiteTelemetry"/> to use. This field is read-only.
        /// </summary>
        private readonly ISiteTelemetry _telemetry;

        /// <summary>
        /// The logger to use. This field is read-only.
        /// </summary>
        private readonly ILogger<DocumentCollectionInitializer> _logger;

        /// <summary>
        /// The name of the Azure DocumentDB database. This field is read-only.
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
        /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
        /// <param name="options">The <see cref="UserStoreOptions"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger{DocumentCollectionInitializer}"/> to use.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="options"/> is invalid.
        /// </exception>
        public DocumentCollectionInitializer(
            ISiteTelemetry telemetry,
            UserStoreOptions options,
            ILogger<DocumentCollectionInitializer> logger)
        {
            _documentClient = DocumentHelpers.CreateClient(options);
            _existingCollections = new ConcurrentDictionary<string, bool>();

            _telemetry = telemetry;
            _logger = logger;
            _databaseName = options.DatabaseName;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _documentClient?.Dispose();
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

            var response = await _telemetry.TrackDocumentDbAsync(
                "CreateDocumentCollectionIfNotExists",
                () =>
                {
                    return _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                        BuildDatabaseUri(),
                        new DocumentCollection() { Id = collectionName },
                        new RequestOptions() { OfferThroughput = 400 });
                },
                IsSuccessfulRequest);

            bool created = response.StatusCode == HttpStatusCode.Created;

            if (created)
            {
                _logger?.LogInformation($"Created collection '{collectionName}' in database '{_databaseName}'.");
            }

            _existingCollections.AddOrUpdate(collectionName, created, (p, r) => true);

            return created;
        }

        /// <summary>
        /// Returns whether the specified resource request was successful.
        /// </summary>
        /// <param name="response">The <see cref="IResourceResponseBase"/> to test for success.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="response"/> was successful; otherwise <see langword="false"/>.
        /// </returns>
        private static bool IsSuccessfulRequest(IResourceResponseBase response)
        {
            return response.StatusCode >= HttpStatusCode.OK && response.StatusCode < HttpStatusCode.BadRequest;
        }

        /// <summary>
        /// Ensures that the database exists as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the database exists.
        /// </returns>
        private async Task EnsureDatabaseExistsAsync()
        {
            var response = await _telemetry.TrackDocumentDbAsync(
                "CreateDatabaseIfNotExists",
                () => _documentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = _databaseName }),
                IsSuccessfulRequest);

            bool created = response.StatusCode == HttpStatusCode.Created;

            if (created)
            {
                _logger?.LogInformation($"Created database '{_databaseName}'.");
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
