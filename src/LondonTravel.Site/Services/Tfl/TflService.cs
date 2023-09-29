// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Extensions.Caching.Memory;
using Refit;

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// A class representing the default implementation of <see cref="ITflService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TflService"/> class.
/// </remarks>
/// <param name="client">The <see cref="ITflClient"/> to use.</param>
/// <param name="cache">The <see cref="IMemoryCache"/> to use.</param>
/// <param name="options">The <see cref="TflOptions"/> to use.</param>
public sealed class TflService(ITflClient client, IMemoryCache cache, TflOptions options) : ITflService
{
    /// <inheritdoc />
    public Task<ICollection<LineInfo>> GetLinesAsync(CancellationToken cancellationToken = default)
    {
        const string CacheKey = "TfL.AvailableLines";
        string supportedModes = string.Join(',', options.SupportedModes ?? Array.Empty<string>());

        return GetWithCachingAsync(
            CacheKey,
            () => client.GetLinesAsync(supportedModes, options.AppId!, options.AppKey!, cancellationToken));
    }

    /// <inheritdoc />
    public Task<ICollection<StopPoint>> GetStopPointsByLineAsync(string lineId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"TfL.{lineId}.StopPoints";

        return GetWithCachingAsync(
            cacheKey,
            () => client.GetStopPointsAsync(lineId, options.AppId!, options.AppKey!, cancellationToken));
    }

    /// <summary>
    /// Calls the specified delegate as an asynchronous operation,
    /// storing the result in the cache if the response is cacheable.
    /// </summary>
    /// <typeparam name="T">The type of the resource to return.</typeparam>
    /// <param name="cacheKey">The cache key to use for the response.</param>
    /// <param name="operation">A delegate to a method to use to get the API response.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
    /// resource of <typeparamref name="T"/> from calling the specified delegate.
    /// </returns>
    private async Task<T> GetWithCachingAsync<T>(string cacheKey, Func<Task<ApiResponse<T>>> operation)
    {
        if (!cache.TryGetValue(cacheKey, out T? result))
        {
            using var response = await operation();
            await response.EnsureSuccessStatusCodeAsync();

            result = response.Content;

            if (!string.IsNullOrEmpty(cacheKey) &&
                response.Headers.CacheControl != null &&
                response.Headers.CacheControl.MaxAge.HasValue)
            {
                cache.Set(cacheKey, result, absoluteExpirationRelativeToNow: response.Headers.CacheControl.MaxAge.Value);
            }
        }

        return result!;
    }
}
