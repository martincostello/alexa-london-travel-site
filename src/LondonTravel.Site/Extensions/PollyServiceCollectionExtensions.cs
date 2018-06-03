// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Polly;
    using Polly.Extensions.Http;
    using Polly.Registry;

    /// <summary>
    /// A class containing Polly-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class PollyServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Polly to the services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add Polly to.</param>
        /// <returns>
        /// The value specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddPolly(this IServiceCollection services)
        {
            var sleepDurations = new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
            };

            var readPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(sleepDurations)
                .WithPolicyKey("ReadPolicy");

            var writePolicy = Policy.NoOpAsync()
                .AsAsyncPolicy<HttpResponseMessage>()
                .WithPolicyKey("WritePolicy");

            var policies = new[]
            {
                readPolicy,
                writePolicy,
            };

            IPolicyRegistry<string> registry = services.AddPolicyRegistry();

            foreach (var policy in policies)
            {
                registry.Add(policy.PolicyKey, policy);
            }

            return services;
        }
    }
}
