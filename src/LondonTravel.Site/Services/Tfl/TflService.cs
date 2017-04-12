// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Tfl
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using Options;

    /// <summary>
    /// A class representing the default implementation of <see cref="ITflService"/>.
    /// </summary>
    public sealed class TflService : ITflService, IDisposable
    {
        /// <summary>
        /// The <see cref="HttpClient"/> to use. This field is read-only.
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// The <see cref="IMemoryCache"/> to use. This field is read-only.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// The <see cref="TflOptions"/> to use. This field is read-only.
        /// </summary>
        private readonly TflOptions _options;

        /// <summary>
        /// Whether the instance has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TflService"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/> to use.</param>
        /// <param name="options">The <see cref="TflOptions"/> to use.</param>
        public TflService(HttpClient httpClient, IMemoryCache cache, TflOptions options)
        {
            _client = httpClient;
            _cache = cache;
            _options = options;

            _client.BaseAddress = _options.BaseUri;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_disposed)
            {
                _client?.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc />
        public async Task<ICollection<LineInfo>> GetLinesAsync(CancellationToken cancellationToken)
        {
            const string CacheKey = "TfL.AvailableLines";
            string relativeUrl = $"Line/Mode/{string.Join(",", _options.SupportedModes)}";
            Uri requestUri = BuildRequestUri(relativeUrl);

            return await GetAsJsonWithCacheAsync<ICollection<LineInfo>>(requestUri, CacheKey, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ICollection<StopPoint>> GetStopPointsByLineAsync(string lineId, CancellationToken cancellationToken)
        {
            string cacheKey = $"TfL.{lineId}.AvailableLines";
            string relativeUrl = $"Line/{lineId}/StopPoints";
            Uri requestUri = BuildRequestUri(relativeUrl);

            return await GetAsJsonWithCacheAsync<ICollection<StopPoint>>(requestUri, cacheKey, cancellationToken);
        }

        /// <summary>
        /// Builds the URI for the specified request URL.
        /// </summary>
        /// <param name="relativeUrl">The relative URL to build the URI for.</param>
        /// <returns>
        /// The <see cref="Uri"/> to use for the specified relative URL with the required query string parameters appended.
        /// </returns>
        private Uri BuildRequestUri(string relativeUrl)
        {
            var builder = new StringBuilder(relativeUrl);

            if (relativeUrl.IndexOf('?') > -1)
            {
                builder.Append('&');
            }
            else
            {
                builder.Append('?');
            }

            builder.Append($"app_id={_options.AppId}&app_key={_options.AppKey}");

            return new Uri(builder.ToString(), UriKind.Relative);
        }

        /// <summary>
        /// Performs an HTTP to the specified URI as an asynchronous operation,
        /// storing the result if the cache if the response is cacheable.
        /// </summary>
        /// <typeparam name="T">The type of the resource to get.</typeparam>
        /// <param name="requestUri">The URI of the resource to get.</param>
        /// <param name="cacheKey">The cache key to use for the resource.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asychronous operation to get the
        /// resource of <typeparamref name="T"/> at the specified request URI.
        /// </returns>
        private async Task<T> GetAsJsonWithCacheAsync<T>(
            Uri requestUri,
            string cacheKey,
            CancellationToken cancellationToken)
        {
            if (!_cache.TryGetValue(cacheKey, out T result))
            {
                using (var response = await _client.GetAsync(requestUri, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    result = JsonConvert.DeserializeObject<T>(json);

                    if (!string.IsNullOrEmpty(cacheKey) &&
                        response.Headers.CacheControl.MaxAge.HasValue)
                    {
                        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(response.Headers.CacheControl.MaxAge.Value);

                        _cache.Set(cacheKey, result, options);
                    }
                }
            }

            return result;
        }
    }
}
