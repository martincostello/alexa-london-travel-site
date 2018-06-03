// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.IO;
    using JustEat.HttpClientInterception;
    using MartinCostello.LondonTravel.Site.Services.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;

    /// <summary>
    /// A class representing a factory for creating instances of the application.
    /// </summary>
    public class TestServerFixture : WebApplicationFactory<Startup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestServerFixture"/> class.
        /// </summary>
        public TestServerFixture()
            : base()
        {
            ClientOptions.AllowAutoRedirect = false;
            ClientOptions.BaseAddress = new Uri("https://localhost");
        }

        /// <summary>
        /// Gets the <see cref="HttpClientInterceptorOptions"/> in use.
        /// </summary>
        public HttpClientInterceptorOptions Interceptor { get; } = new HttpClientInterceptorOptions() { ThrowOnMissingRegistration = true };

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(
                (services) => services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Interceptor)));

            builder.ConfigureServices(
                (services) =>
                {
                    services.AddSingleton<InMemoryDocumentStore>();
                    services.AddSingleton<IDocumentClient>((p) => p.GetRequiredService<InMemoryDocumentStore>());
                    services.AddSingleton<IDocumentCollectionInitializer>((p) => p.GetRequiredService<InMemoryDocumentStore>());
                });

            builder.ConfigureAppConfiguration(ConfigureTests);
        }

        private static void ConfigureTests(IConfigurationBuilder builder)
        {
            // Remove the application's normal configuration
            builder.Sources.Clear();

            string directory = Path.GetDirectoryName(typeof(TestServerFixture).Assembly.Location);
            string fullPath = Path.Combine(directory, "testsettings.json");

            // Apply new configuration for tests
            builder.AddJsonFile("appsettings.json")
                   .AddJsonFile(fullPath)
                   .AddEnvironmentVariables();
        }
    }
}
