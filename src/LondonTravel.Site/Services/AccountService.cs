// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Services.Data;
using Microsoft.Extensions.Caching.Memory;

namespace MartinCostello.LondonTravel.Site.Services;

public sealed partial class AccountService : IAccountService
{
    private readonly IDocumentService _service;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

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
                Log.FailedToFindUserByAccessToken(_logger, ex);
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

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Failed to find user by access token.")]
        public static partial void FailedToFindUserByAccessToken(ILogger logger, Exception exception);
    }
}
