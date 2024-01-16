// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Services.Tfl;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Refit;

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
                .ConfigureHttpClientDefaults((p) => p.ApplyDefaultConfiguration());

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<SiteOptions>();

        if (options.Authentication?.ExternalProviders is { } providers)
        {
            foreach (string name in providers.Keys)
            {
                services.AddHttpClient(name)
                        .ApplyRemoteAuthenticationConfiguration();
            }
        }

        services.AddHttpClient<ITflClient, ITflClient>(AddTfl);

        services.AddSingleton<IHttpContentSerializer>((p) =>
        {
            var options = p.GetRequiredService<IOptions<JsonOptions>>().Value;
            return new SystemTextJsonContentSerializer(options.SerializerOptions);
        });

        return services;
    }

    /// <summary>
    /// Adds a typed client for the TfL API.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> to configure the client with.</param>
    /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
    /// <returns>
    /// The <see cref="ITflClient"/> to use.
    /// </returns>
    private static ITflClient AddTfl(HttpClient client, IServiceProvider provider)
    {
        client.BaseAddress = provider.GetRequiredService<TflOptions>().BaseUri;

        var settings = new RefitSettings()
        {
            ContentSerializer = provider.GetRequiredService<IHttpContentSerializer>(),
            HttpMessageHandlerFactory = () => provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(),
        };

        return RestService.For<ITflClient>(client, settings);
    }
}
