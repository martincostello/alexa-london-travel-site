// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration
{
    /// <summary>
    /// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
    /// </summary>
    public sealed class HttpServerFixture : TestServerFixture
    {
        private IHost? _host;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
        /// </summary>
        public HttpServerFixture()
            : base()
        {
            Interceptor.RegisterBundle(Path.Combine("Integration", "oauth-http-bundle.json"));
            Interceptor.RegisterBundle(Path.Combine("Integration", "tfl-http-bundle.json"));
        }

        /// <summary>
        /// Gets the server address of the application.
        /// </summary>
        public Uri ServerAddress
        {
            get
            {
                EnsureServer();
                return ClientOptions.BaseAddress;
            }
        }

        /// <inheritdoc />
        public override IServiceProvider Services
        {
            get
            {
                EnsureServer();
                return _host!.Services!;
            }
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
                CheckCertificateRevocationList = true,
                MaxAutomaticRedirections = ClientOptions.MaxAutomaticRedirections,
                UseCookies = ClientOptions.HandleCookies,
            };

            if (ClientOptions.BaseAddress.IsLoopback &&
                string.Equals(ClientOptions.BaseAddress.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            var client = new HttpClient(handler, disposeHandler: true);

            ConfigureClient(client);

            client.BaseAddress = ClientOptions.BaseAddress;

            return client;
        }

        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            // Intercept remote authentication to redirect locally for browser UI tests
            builder.ConfigureServices((p) =>
            {
                p.AddSingleton<ConfigureAuthenticationHandlers>();
                p.AddSingleton<IPostConfigureOptions<AmazonAuthenticationOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<AppleAuthenticationOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<FacebookOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<GoogleOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<MicrosoftAccountOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
                p.AddSingleton<IPostConfigureOptions<TwitterOptions>>((r) => r.GetRequiredService<ConfigureAuthenticationHandlers>());
            });

            builder.ConfigureKestrel(
                (p) => p.ConfigureHttpsDefaults(
                    (r) => r.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            // Configure the server address for the server to
            // listen on for HTTPS requests on a dynamic port.
            builder.UseUrls("https://127.0.0.1:0");

            // Allow the tests on the self-hosted server to link accounts via "Amazon"
            builder.ConfigureAppConfiguration((p) => p.Add(new HttpServerFixtureConfigurationSource(this)));
        }

        /// <inheritdoc />
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureWebHost((p) => p.UseKestrel());

            _host = builder.Build();
            _host.Start();

            var server = _host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            ClientOptions.BaseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();

            // Force the configuration to reload now the server address is assigned
            var config = _host.Services.GetRequiredService<IConfiguration>();

            if (config is IConfigurationRoot root)
            {
                root.Reload();
            }

            // The base class still needs a separate host using TestServer
            var testHostBuilder = CreateHostBuilder();
            var testHost = testHostBuilder!
                .ConfigureWebHost((p) => p.UseTestServer())
                .Build();

            testHost.Start();

            return testHost;
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

        private void EnsureServer()
        {
            if (_host is null)
            {
                using (CreateDefaultClient())
                {
                }
            }
        }

        private sealed class HttpServerFixtureConfigurationSource : IConfigurationSource
        {
            internal HttpServerFixtureConfigurationSource(HttpServerFixture fixture)
            {
                Fixture = fixture;
            }

            private HttpServerFixture Fixture { get; }

            public IConfigurationProvider Build(IConfigurationBuilder builder)
            {
                return new HttpServerFixtureConfigurationProvider(Fixture);
            }
        }

        private sealed class HttpServerFixtureConfigurationProvider : ConfigurationProvider
        {
            internal HttpServerFixtureConfigurationProvider(HttpServerFixture fixture)
            {
                Fixture = fixture;
            }

            private HttpServerFixture Fixture { get; }

            public override void Load()
            {
                Data = new Dictionary<string, string>()
                {
                    ["Site:Alexa:RedirectUrls:3"] = Fixture.ClientOptions.BaseAddress.ToString() + "manage/",
                };
            }
        }
    }
}
