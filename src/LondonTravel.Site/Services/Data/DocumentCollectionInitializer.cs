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
        /// The logger to use. This field is read-only.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The name of the Azure DocumentDB database. This field is read-only.
        /// </summary>
        private readonly string _databaseName;

        /// <summary>
        /// The collections that have been checked to exist. This field is read-only.
        /// </summary>
        private readonly ConcurrentDictionary<string, bool> _existingCollections;

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
        public DocumentCollectionInitializer(
            UserStoreOptions options,
            ILogger<DocumentCollectionInitializer> logger)
        {
            _existingCollections = new ConcurrentDictionary<string, bool>();
            _logger = logger;
            _databaseName = options.DatabaseName;
        }

        /// <inheritdoc />
        public async Task<bool> EnsureCollectionExistsAsync(IDocumentClient client, string collectionName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (collectionName == null)
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            if (_existingCollections.ContainsKey(collectionName))
            {
                return true;
            }

            await EnsureDatabaseExistsAsync(client);

            Uri uri = BuildDatabaseUri();
            Uri createUri = new Uri($"{uri}/{DocumentHelpers.CollectionsUriFragment}", UriKind.Relative);

            var response = await client.CreateDocumentCollectionIfNotExistsAsync(
                uri,
                new DocumentCollection() { Id = collectionName },
                new RequestOptions() { OfferThroughput = 400 });

            bool created = response.StatusCode == HttpStatusCode.Created;

            if (created)
            {
                _logger.LogInformation("Created collection {CollectionName} in database {DatabaseName}.", collectionName, _databaseName);
            }

            _existingCollections.AddOrUpdate(collectionName, created, (p, r) => true);

            return created;
        }

        /// <summary>
        /// Ensures that the database exists as an asynchronous operation.
        /// </summary>
        /// <param name="client">The document client to use to ensure the database exists.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the database exists.
        /// </returns>
        private async Task EnsureDatabaseExistsAsync(IDocumentClient client)
        {
            var response = await client.CreateDatabaseIfNotExistsAsync(new Database() { Id = _databaseName });

            bool created = response.StatusCode == HttpStatusCode.Created;

            if (created)
            {
                _logger.LogInformation("Created database {DatabaseName}.", _databaseName);
            }
        }

        /// <summary>
        /// Builds a URI for the database.
        /// </summary>
        /// <returns>
        /// The URI to use for the database.
        /// </returns>
        private Uri BuildDatabaseUri() => UriFactory.CreateDatabaseUri(_databaseName);
    }
}
