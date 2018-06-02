// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Integration
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.TestHost;

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
            var builder = CreateWebHostBuilder()
                .UseSolutionRelativeContentRoot("src/LondonTravel.Site");

            ConfigureWebHost(builder);

            _webHost = builder.Build();
            _webHost.Start();

            ClientOptions.BaseAddress = new Uri(_webHost.ServerFeatures.Get<IServerAddressesFeature>().Addresses.LastOrDefault());
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

            var client = new HttpClient(handler);

            ConfigureClient(client);

            client.BaseAddress = ClientOptions.BaseAddress;

            return client;
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
    }
}
