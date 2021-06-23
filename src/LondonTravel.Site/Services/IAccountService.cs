// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;

namespace MartinCostello.LondonTravel.Site.Services
{
    /// <summary>
    /// Defines operations for account management.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Gets the user with the specified access token, if any, as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token to find the user for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the user
        /// with the specified access token, if found, otherwise <see langword="null"/>.
        /// </returns>
        Task<LondonTravelUser?> GetUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the number of registered users as an asynchronous operation.
        /// </summary>
        /// <param name="useCache">Whether to use the cached value of users.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the number of registered users.
        /// </returns>
        Task<long> GetUserCountAsync(bool useCache);
    }
}
