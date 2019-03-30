// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Options;
    using MartinCostello.LondonTravel.Site.Services.Data;
    using Microsoft.Azure.Documents;

    /// <summary>
    /// A class representing an in-memory document store. This class cannot be inherited.
    /// </summary>
    internal sealed class InMemoryDocumentStore : IDocumentService, IDocumentCollectionInitializer
    {
        private readonly IDictionary<string, DocumentCollection> _collections;

        public InMemoryDocumentStore(UserStoreOptions options)
        {
            _collections = new Dictionary<string, DocumentCollection>();
            CollectionName = options.CollectionName;
        }

        private string CollectionName { get; }

        /// <summary>
        /// Clears the document store.
        /// </summary>
        public void Clear()
        {
            foreach (var collection in _collections.Values)
            {
                collection.Clear();
            }

            _collections.Clear();
        }

        /// <inheritdoc />
        public Task<string> CreateAsync(object document)
        {
            DocumentCollection collection = EnsureCollection();
            string id = collection.Create(document);

            return Task.FromResult(id);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(string id)
        {
            DocumentCollection collection = EnsureCollection();
            bool deleted = collection.Delete(id);

            return Task.FromResult(deleted);
        }

        /// <inheritdoc />
        public Task<bool> EnsureCollectionExistsAsync(IDocumentClient client, string collectionName)
        {
            bool exists = _collections.TryGetValue(collectionName, out var _);

            if (!exists)
            {
                _collections[collectionName] = new DocumentCollection();
            }

            return Task.FromResult(!exists);
        }

        /// <inheritdoc />
        public Task<T> GetAsync<T>(string id)
            where T : class
        {
            DocumentCollection collection = EnsureCollection();
            T item = collection.Get<T>(id);

            return Task.FromResult(item);
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
            where T : class
        {
            DocumentCollection collection = EnsureCollection();

            var results = collection.Get<T>()
                .Where(predicate)
                .AsEnumerable();

            return Task.FromResult(results);
        }

        /// <inheritdoc />
        public Task<long> GetDocumentCountAsync()
        {
            DocumentCollection collection = EnsureCollection();
            long count = collection.Count();

            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<T> ReplaceAsync<T>(string id, T document, string etag)
            where T : class
        {
            DocumentCollection collection = EnsureCollection();
            document = collection.Replace(id, document, etag);

            return Task.FromResult(document);
        }

        private DocumentCollection EnsureCollection()
        {
            if (!_collections.TryGetValue(CollectionName, out var collection))
            {
                _collections[CollectionName] = collection = new DocumentCollection();
            }

            return collection;
        }

        private sealed class DocumentEntry
        {
            internal DocumentEntry(string id, object value, string etag)
            {
                ETag = etag;
                Id = id;
                Value = value;
            }

            internal string ETag { get; set; }

            internal string Id { get; }

            internal object Value { get; set; }
        }

        private sealed class DocumentCollection
        {
            private readonly IDictionary<Type, IDictionary<string, DocumentEntry>> _documents;

            internal DocumentCollection()
            {
                _documents = new Dictionary<Type, IDictionary<string, DocumentEntry>>();
            }

            internal void Clear()
            {
                foreach (var subset in _documents)
                {
                    subset.Value.Clear();
                }

                _documents.Clear();
            }

            internal long Count() => _documents.Values.Sum((p) => p.Count);

            internal string Create(object document)
            {
                Type type = document.GetType();

                if (!_documents.TryGetValue(type, out var subset))
                {
                    subset = _documents[type] = new Dictionary<string, DocumentEntry>();
                }

                string id = Guid.NewGuid().ToString();
                string etag = Guid.NewGuid().ToString();

                subset[id] = new DocumentEntry(id, document, etag);

                if (document is Identity.LondonTravelUser user)
                {
                    user.ETag = etag;
                }

                return id;
            }

            internal bool Delete(string id)
            {
                foreach (var subset in _documents.Values)
                {
                    if (subset.Remove(id))
                    {
                        return true;
                    }
                }

                return false;
            }

            internal T Get<T>(string id)
            {
                if (!_documents.TryGetValue(typeof(T), out var subset) || !subset.TryGetValue(id, out var document))
                {
                    throw new InvalidOperationException($"Document '{id}' not found.");
                }

                return (T)document.Value;
            }

            internal IQueryable<T> Get<T>()
            {
                if (!_documents.TryGetValue(typeof(T), out var subset))
                {
                    return Array.Empty<T>().AsQueryable();
                }

                return subset
                    .Select((p) => p.Value.Value)
                    .OfType<T>()
                    .AsQueryable();
            }

            internal T Replace<T>(string id, T value, string etag)
            {
                if (!_documents.TryGetValue(typeof(T), out var subset) || !subset.TryGetValue(id, out var document))
                {
                    throw new InvalidOperationException($"Document '{id}' not found.");
                }

                if (!string.Equals(etag, document.ETag, StringComparison.OrdinalIgnoreCase))
                {
                    return default;
                }

                document.ETag = Guid.NewGuid().ToString();
                document.Value = value;

                if (value is Identity.LondonTravelUser user)
                {
                    user.ETag = document.ETag;
                }

                return value;
            }
        }
    }
}
