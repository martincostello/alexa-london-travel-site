// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Tfl
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Refit;

    /// <summary>
    /// Defines an HTTP client for the TfL API.
    /// </summary>
    public interface ITflClient
    {
        /// <summary>
        /// Gets the lines for the specified modes of travel as an asynchronous operation.
        /// </summary>
        /// <param name="supportedModes">A comma-separated list of supported modes of travel.</param>
        /// <param name="applicationId">The application Id to use.</param>
        /// <param name="applicationKey">The application key to use.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
        /// lines associated with the modes specified by <paramref name="supportedModes"/>.
        /// </returns>
        [Get("/Line/Mode/{supportedModes}")]
        Task<ApiResponse<ICollection<LineInfo>>> GetLinesAsync(
            string supportedModes,
            [AliasAs("app_id")] string applicationId,
            [AliasAs("app_key")] string applicationKey,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets the stop points for the specified line Id as an asynchronous operation.
        /// </summary>
        /// <param name="lineId">The line Id to get the stop points for.</param>
        /// <param name="applicationId">The application Id to use.</param>
        /// <param name="applicationKey">The application key to use.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
        /// stop points associated with the line specified by <paramref name="lineId"/>.
        /// </returns>
        [Get("/Line/{lineId}/StopPoints")]
        Task<ApiResponse<ICollection<StopPoint>>> GetStopPointsAsync(
            string lineId,
            [AliasAs("app_id")] string applicationId,
            [AliasAs("app_key")] string applicationKey,
            CancellationToken cancellationToken);
    }
}
