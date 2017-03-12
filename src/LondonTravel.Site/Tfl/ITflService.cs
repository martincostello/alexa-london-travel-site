// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Tfl
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines the TfL API service.
    /// </summary>
    public interface ITflService : IDisposable
    {
        /// <summary>
        /// Gets the available lines as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous
        /// operation to get the available lines.
        /// </returns>
        Task<JArray> GetLinesAsync(CancellationToken cancellationToken);
    }
}
