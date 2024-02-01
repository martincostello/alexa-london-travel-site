// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Services.Tfl;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing HTTP-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class HttpServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP clients to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>
    /// The value specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient()
                .ApplyDefaultConfiguration();

        //// TODO Re-enable when fixed in .NET 8.0.2
        ////.ConfigureHttpClientDefaults((p) => p.ApplyDefaultConfiguration());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<SiteOptions>();

        if (options.Authentication?.ExternalProviders is { } providers)
        {
            foreach (string name in providers.Keys)
            {
                services.AddHttpClient(name)
                        .ApplyDefaultConfiguration()
                        .ApplyRemoteAuthenticationConfiguration();
            }
        }

        services.AddHttpClient<ITflService, TflService>("TfL", (provider, client) =>
        {
            client.BaseAddress = provider.GetRequiredService<TflOptions>().BaseUri;
        }).ApplyDefaultConfiguration();

        return services;
    }
}
