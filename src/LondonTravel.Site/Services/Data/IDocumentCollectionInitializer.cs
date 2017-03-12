// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Data
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a document collection initializer.
    /// </summary>
    public interface IDocumentCollectionInitializer : IDisposable
    {
        /// <summary>
        /// Ensures that the specified collection exists as an asynchronous operation.
        /// </summary>
        /// <param name="collectionName">The name of the collection to ensure exists.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation to ensure the collection
        /// exists which returns <see langword=""/> if the collection was created by the method
        /// invocation; otherwise <see langword="false"/> if it already existed.
        /// </returns>
        Task<bool> EnsureCollectionExistsAsync(string collectionName);
    }
}
