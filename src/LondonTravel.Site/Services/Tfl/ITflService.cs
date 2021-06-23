// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MartinCostello.LondonTravel.Site.Services.Tfl
{
    /// <summary>
    /// Defines the TfL API service.
    /// </summary>
    public interface ITflService
    {
        /// <summary>
        /// Gets the available lines as an asynchronous operation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous
        /// operation to get the available lines.
        /// </returns>
        Task<ICollection<LineInfo>> GetLinesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the available stop points for the specified line Id as an asynchronous operation.
        /// </summary>
        /// <param name="lineId">The Id of the line to get the stop points for.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get
        /// the available stop points for the line specified by <paramref name="lineId"/>.
        /// </returns>
        Task<ICollection<StopPoint>> GetStopPointsByLineAsync(string lineId, CancellationToken cancellationToken);
    }
}
