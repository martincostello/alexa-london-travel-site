// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System.Linq;
    using Microsoft.AspNetCore.ApplicationInsights.HostingStartup;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Removes the registered Application Insights JavaScript tag helper.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection RemoveApplicationInsightsTagHelper(this IServiceCollection services)
        {
            // See https://github.com/aspnet/AzureIntegration/issues/88
            var copy = services
                .Where((p) => p.ImplementationType == typeof(JavaScriptSnippetTagHelperComponent))
                .ToArray();

            foreach (var item in copy)
            {
                services.Remove(item);
            }

            return services;
        }
    }
}
