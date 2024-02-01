// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization.Metadata;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// A class representing the default implementation of <see cref="ITflService"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TflService"/> class.
/// </remarks>
/// <param name="client">The <see cref="HttpClient"/> to use.</param>
/// <param name="cache">The <see cref="IMemoryCache"/> to use.</param>
/// <param name="options">The <see cref="TflOptions"/> to use.</param>
public sealed class TflService(HttpClient client, IMemoryCache cache, TflOptions options) : ITflService
{
    /// <inheritdoc />
    public async Task<ICollection<LineInfo>> GetLinesAsync(CancellationToken cancellationToken = default)
    {
        string supportedModes = string.Join(',', options.SupportedModes ?? []);
        supportedModes = Uri.EscapeDataString(supportedModes);

        var requestUri = CreateUri($"Line/Mode/{supportedModes}");

        return await GetWithCachingAsync(
            "TfL.AvailableLines",
            ApplicationJsonSerializerContext.Default.ICollectionLineInfo,
            () => client.GetAsync(requestUri, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<ICollection<StopPoint>> GetStopPointsByLineAsync(string lineId, CancellationToken cancellationToken = default)
    {
        var requestUri = CreateUri($"/Line/{lineId}/StopPoints");

        return await GetWithCachingAsync(
            $"TfL.{lineId}.StopPoints",
            ApplicationJsonSerializerContext.Default.ICollectionStopPoint,
            () => client.GetAsync(requestUri, cancellationToken));
    }

    /// <summary>
    /// Calls the specified delegate as an asynchronous operation,
    /// storing the result in the cache if the response is cacheable.
    /// </summary>
    /// <typeparam name="T">The type of the resource to return.</typeparam>
    /// <param name="cacheKey">The cache key to use for the response.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> to use the deserialize the response.</param>
    /// <param name="operation">A delegate to a method to use to get the API response.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation to get the
    /// resource of <typeparamref name="T"/> from calling the specified delegate.
    /// </returns>
    private async Task<T> GetWithCachingAsync<T>(
        string cacheKey,
        JsonTypeInfo<T> jsonTypeInfo,
        Func<Task<HttpResponseMessage>> operation)
    {
        if (!cache.TryGetValue(cacheKey, out T? result))
        {
            using var response = await operation();
            response.EnsureSuccessStatusCode();

            result = await response.Content.ReadFromJsonAsync(jsonTypeInfo);

            if (!string.IsNullOrEmpty(cacheKey) &&
                response.Headers.CacheControl != null &&
                response.Headers.CacheControl.MaxAge.HasValue)
            {
                cache.Set(cacheKey, result, absoluteExpirationRelativeToNow: response.Headers.CacheControl.MaxAge.Value);
            }
        }

        return result!;
    }

    private Uri CreateUri(string path)
    {
        string query = QueryHelpers.AddQueryString(
            string.Empty,
            [
                KeyValuePair.Create("app_id", options.AppId),
                KeyValuePair.Create("app_key", options.AppKey),
            ]);

        var builder = new UriBuilder()
        {
            Path = path,
            Query = query,
        };

        return new(client.BaseAddress!, builder.Uri.PathAndQuery);
    }
}
