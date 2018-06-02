// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Net.Http;
    using MartinCostello.LondonTravel.Site.Services.Tfl;
    using Microsoft.Extensions.DependencyInjection;
    using Options;
    using Refit;

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
            services
                .AddHttpClient(Microsoft.Extensions.Options.Options.DefaultName)
                .ApplyDefaultConfiguration();

            services
                .AddHttpClient<ITflClient>()
                .AddTypedClient(AddTfl)
                .ApplyDefaultConfiguration();

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
            var options = provider.GetRequiredService<TflOptions>();

            client.BaseAddress = options.BaseUri;

            return RestService.For<ITflClient>(client);
        }
    }
}
