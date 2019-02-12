// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    /// <summary>
    /// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
    /// </summary>
    public sealed class HttpServerFixture : TestServerFixture, IAsyncLifetime
    {
        private IWebHost _host;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
            : base()
        {
        }

        /// <summary>
        /// Gets the server address of the application.
        /// </summary>
        public Uri ServerAddress => ClientOptions.BaseAddress;

        /// <inheritdoc />
        public override IServiceProvider Services => _host?.Services;

        /// <inheritdoc />
        async Task IAsyncLifetime.InitializeAsync()
            => await EnsureHttpServerAsync();

        /// <inheritdoc />
        async Task IAsyncLifetime.DisposeAsync()
        {
            await _host?.StopAsync(default);
            _host?.Dispose();
            _host = null;
        }

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
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureServices(ConfigureServicesForTests);

            builder.ConfigureKestrel(
                (p) => p.ConfigureHttpsDefaults(
                    (r) => r.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            builder.UseUrls(ServerAddress.ToString());

            // Allow the tests on the self-hosted server to link accounts via "Amazon"
            builder.UseSetting("Site:Alexa:RedirectUrls:3", ServerAddress.ToString() + "manage/");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    _host?.Dispose();
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
        }

        private async Task EnsureHttpServerAsync()
        {
            if (_host == null)
            {
                await CreateHttpServer();
            }
        }

        private async Task CreateHttpServer()
        {
            // Configure the server address for the server to listen on for HTTP requests
            ClientOptions.BaseAddress = FindFreeServerAddress();

            var builder = CreateWebHostBuilder();

            ConfigureWebHost(builder);

            _host = builder.Build();
            await _host.StartAsync();
        }
    }
}
