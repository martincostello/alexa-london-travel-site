// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.IO;
    using JustEat.HttpClientInterception;
    using MartinCostello.Logging.XUnit;
    using MartinCostello.LondonTravel.Site.Services.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Http;
    using Microsoft.Extensions.Logging;
    using Xunit.Abstractions;

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

            // HACK Force HTTP server startup
            using (CreateDefaultClient())
            {
            }
        }

        /// <summary>
        /// Gets the <see cref="HttpClientInterceptorOptions"/> in use.
        /// </summary>
        public HttpClientInterceptorOptions Interceptor { get; } = new HttpClientInterceptorOptions() { ThrowOnMissingRegistration = true };

        /// <summary>
        /// Clears the current <see cref="ITestOutputHelper"/>.
        /// </summary>
        public virtual void ClearOutputHelper()
            => Server.Host.Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper = null;

        /// <summary>
        /// Sets the <see cref="ITestOutputHelper"/> to use.
        /// </summary>
        /// <param name="value">The <see cref="ITestOutputHelper"/> to use.</param>
        public virtual void SetOutputHelper(ITestOutputHelper value)
            => Server.Host.Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper = value;

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
                    services.AddSingleton<IDocumentService>((p) => p.GetRequiredService<InMemoryDocumentStore>());
                    services.AddSingleton<IDocumentCollectionInitializer>((p) => p.GetRequiredService<InMemoryDocumentStore>());
                });

            // Disable dependency tracking to work around https://github.com/Microsoft/ApplicationInsights-dotnet-server/pull/1006
            builder.ConfigureServices(
                (services) => services.DisableApplicationInsights());

            builder.ConfigureAppConfiguration(ConfigureTests)
                   .ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit());
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
