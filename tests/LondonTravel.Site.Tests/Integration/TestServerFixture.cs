// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
using AspNet.Security.OAuth.Apple;
using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using MartinCostello.LondonTravel.Site.Services.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Site.Integration
{
    /// <summary>
    /// A class representing a factory for creating instances of the application.
    /// </summary>
    public class TestServerFixture : WebApplicationFactory<Startup>, ITestOutputHelperAccessor
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
        public HttpClientInterceptorOptions Interceptor { get; }
            = new HttpClientInterceptorOptions()
                 .ThrowsOnMissingRegistration()
                 .RegisterBundle(Path.Combine("Integration", "oauth-http-bundle.json"));

        /// <inheritdoc />
        public ITestOutputHelper? OutputHelper { get; set; }

        /// <summary>
        /// Clears the current <see cref="ITestOutputHelper"/>.
        /// </summary>
        public virtual void ClearOutputHelper()
        {
            OutputHelper = null;
        }

        /// <summary>
        /// Sets the <see cref="ITestOutputHelper"/> to use.
        /// </summary>
        /// <param name="value">The <see cref="ITestOutputHelper"/> to use.</param>
        public virtual void SetOutputHelper(ITestOutputHelper value)
        {
            OutputHelper = value;
        }

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

                    // Configure a test private key for Sign in with Apple
                    services
                        .AddOptions<AppleAuthenticationOptions>("Apple")
                        .Configure((options) =>
                        {
                            options.GenerateClientSecret = true;
                            options.ValidateTokens = false;
                            options.PrivateKeyBytes = async (keyId, cancellationToken) =>
                            {
                                string privateKey = await File.ReadAllTextAsync(
                                    Path.Combine("Integration", "apple-test-cert.p8"),
                                    cancellationToken);

                                if (privateKey.StartsWith("-----BEGIN PRIVATE KEY-----", StringComparison.Ordinal))
                                {
                                    string[] lines = privateKey.Split('\n');
                                    privateKey = string.Join(string.Empty, lines[1..^1]);
                                }

                                return Convert.FromBase64String(privateKey);
                            };
                        });
                });

            builder.ConfigureAppConfiguration(ConfigureTests)
                   .ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this))
                   .UseSolutionRelativeContentRoot(Path.Combine("src", "LondonTravel.Site"));
        }

        private void ConfigureTests(IConfigurationBuilder builder)
        {
            string? directory = Path.GetDirectoryName(typeof(TestServerFixture).Assembly.Location);
            string fullPath = Path.Combine(directory ?? ".", "testsettings.json");

            builder.AddJsonFile(fullPath);
        }
    }
}
