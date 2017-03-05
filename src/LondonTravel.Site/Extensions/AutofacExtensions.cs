// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A class containing extension methods for configuring Autofac. This class cannot be inherited.
    /// </summary>
    public static class AutofacExtensions
    {
        /// <summary>
        /// Registers Autofac for the specified <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add Autofac for.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddAutofac(this IServiceCollection services)
        {
            return services.AddSingleton<IServiceProviderFactory<ContainerBuilder>, AutofacServiceProviderFactory>();
        }

        /// <summary>
        /// Registers Autofac for the specified <see cref="IWebHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to use Autofac for.</param>
        /// <returns>
        /// The <see cref="IWebHostBuilder"/> specified by <paramref name="builder"/>.
        /// </returns>
        public static IWebHostBuilder UseAutofac(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((p) => p.AddAutofac());
        }

        /// <summary>
        /// A class representing the <see cref="IServiceProviderFactory{T}"/> to use for Autofac. This class cannot be inherited.
        /// </summary>
        private sealed class AutofacServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
        {
            /// <inheritdoc />
            public ContainerBuilder CreateBuilder(IServiceCollection services)
            {
                var builder = new ContainerBuilder();

                builder.Populate(services);

                return builder;
            }

            /// <inheritdoc />
            public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
            {
                return new AutofacServiceProvider(containerBuilder.Build());
            }
        }
    }
}
