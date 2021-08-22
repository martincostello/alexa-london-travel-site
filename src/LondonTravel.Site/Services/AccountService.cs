// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Services.Data;
using Microsoft.Extensions.Caching.Memory;

namespace MartinCostello.LondonTravel.Site.Services;

public class AccountService : IAccountService
{
    /// <summary>
    /// The <see cref="IDocumentService"/> to use. This field is read-only.
    /// </summary>
    private readonly IDocumentService _service;

    /// <summary>
    /// The <see cref="IMemoryCache"/> to use. This field is read-only.
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// The <see cref="ILogger"/> to use. This field is read-only.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountService"/> class.
    /// </summary>
    /// <param name="service">The <see cref="IDocumentService"/> to use.</param>
    /// <param name="cache">The <see cref="IMemoryCache"/> to use.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public AccountService(IDocumentService service, IMemoryCache cache, ILogger<AccountService> logger)
    {
        _service = service;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LondonTravelUser?> GetUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        LondonTravelUser? user = null;

        if (!string.IsNullOrEmpty(accessToken))
        {
            try
            {
                user = (await _service.GetAsync((p) => p.AlexaToken == accessToken, cancellationToken)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, "Failed to find user by access token.");
                throw;
            }
        }

        return user;
    }

    public Task<long> GetUserCountAsync(bool useCache)
        => useCache ? GetUserCountFromCacheAsync() : GetUserCountFromDocumentStoreAsync();

    private async Task<long> GetUserCountFromCacheAsync()
    {
        const string CacheKey = "DocumentStore.Count";

        if (!_cache.TryGetValue(CacheKey, out long count))
        {
            count = await GetUserCountFromDocumentStoreAsync();

            _cache.Set(CacheKey, count, TimeSpan.FromHours(12));
        }

        return count;
    }

    private Task<long> GetUserCountFromDocumentStoreAsync()
        => _service.GetDocumentCountAsync();
}
