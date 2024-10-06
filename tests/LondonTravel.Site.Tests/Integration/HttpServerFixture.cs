// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography.X509Certificates;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Site.Extensions;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration;

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
            string.Equals(ClientOptions.BaseAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        var client = new HttpClient(handler, disposeHandler: true);

        ConfigureClient(client);

        client.BaseAddress = ServerAddress;

        return client;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Intercept remote authentication to redirect locally for browser UI tests
        builder.ConfigureServices((p) =>
        {
            static ConfigureAuthenticationHandlers ResolvePostConfigureOptions(IServiceProvider serviceProvider)
                => serviceProvider.GetRequiredService<ConfigureAuthenticationHandlers>();

            p.AddSingleton<ConfigureAuthenticationHandlers>();
            p.AddSingleton<IPostConfigureOptions<AmazonAuthenticationOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<AppleAuthenticationOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<GoogleOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<MicrosoftAccountOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<TwitterOptions>>(ResolvePostConfigureOptions);
        });

        builder.ConfigureKestrel(
            (p) => p.ConfigureHttpsDefaults(
                (r) => r.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile("localhost-dev.pfx", "Pa55w0rd!")));

        // Configure the server address for the server to
        // listen on for HTTPS requests on a dynamic port.
        builder.UseUrls("https://127.0.0.1:0");

        // Allow the tests on the self-hosted server to link accounts via "Amazon"
        builder.ConfigureAppConfiguration((p) => p.Add(new HttpServerFixtureConfigurationSource(this)));
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        _host = builder.Build();
        _host.Start();

        ClientOptions.BaseAddress = _host.GetAddress();

        // Force the configuration to reload now the server address is assigned
        var config = _host.Services.GetRequiredService<IConfiguration>();

        if (config is IConfigurationRoot root)
        {
            root.Reload();
        }

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
                try
                {
                    _host?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // HACK Workaround double dispose issue.
                    // See https://github.com/dotnet/aspnetcore/pull/37631.
                }
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
            Data = new Dictionary<string, string?>()
            {
                ["Site:Alexa:RedirectUrls:3"] = $"{Fixture.ClientOptions.BaseAddress}manage/",
            };
        }
    }
}
