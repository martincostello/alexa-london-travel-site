// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using MartinCostello.Logging.XUnit;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit.Abstractions;

    /// <summary>
    /// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
    /// </summary>
    public sealed class HttpServerFixture : TestServerFixture
    {
        private readonly IWebHost _webHost;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
            : base()
        {
            ClientOptions.BaseAddress = FindFreeServerAddress();

            string applicationBasePath = AppContext.BaseDirectory;

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")))
            {
                applicationBasePath = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
            }

            var builder = CreateWebHostBuilder()
                .UseSolutionRelativeContentRoot("src/LondonTravel.Site", applicationBasePath: applicationBasePath)
                .UseUrls(ClientOptions.BaseAddress.ToString())
                .UseKestrel(
                    (p) => p.ConfigureHttpsDefaults(
                        (r) => r.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            ConfigureWebHost(builder);

            _webHost = builder.Build();
            _webHost.Start();
        }

        /// <summary>
        /// Gets the server address of the application.
        /// </summary>
        public Uri ServerAddress => ClientOptions.BaseAddress;

        /// <summary>
        /// Creates an <see cref="HttpClient"/> to communicate with the application.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpClient"/> that can be to used to make application requests.
        /// </returns>
        public HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = ClientOptions.AllowAutoRedirect,
                MaxAutomaticRedirections = ClientOptions.MaxAutomaticRedirections,
                UseCookies = ClientOptions.HandleCookies,
            };

            if (ClientOptions.BaseAddress.IsLoopback &&
                string.Equals(ClientOptions.BaseAddress.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                handler.ServerCertificateCustomValidationCallback = (request, cert, chain, errors) => true;
            }

            var client = new HttpClient(handler);

            ConfigureClient(client);

            client.BaseAddress = ClientOptions.BaseAddress;

            return client;
        }

        /// <inheritdoc />
        public override void ClearOutputHelper()
            => _webHost.Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper = null;

        /// <inheritdoc />
        public override void SetOutputHelper(ITestOutputHelper value)
            => _webHost.Services.GetRequiredService<ITestOutputHelperAccessor>().OutputHelper = value;

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(ConfigureServicesForTests);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    _webHost?.Dispose();
                }

                _disposed = true;
            }
        }

        private static Uri FindFreeServerAddress()
        {
            int port = GetFreePortNumber();

            return new UriBuilder()
            {
                Scheme = "https",
                Host = "localhost",
                Port = port,
            }.Uri;
        }

        private static int GetFreePortNumber()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();

            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private void ConfigureServicesForTests(IServiceCollection services)
        {
            // Intercept remote authentication to redirect locally for browser UI tests
            services.AddSingleton<IStartupFilter, RemoteAuthorizationEventsFilter>(
                (_) => new RemoteAuthorizationEventsFilter(ServerAddress));

            // Disable dependency tracking to work around https://github.com/Microsoft/ApplicationInsights-dotnet-server/pull/1006
            services.DisableApplicationInsights();
        }
    }
}
