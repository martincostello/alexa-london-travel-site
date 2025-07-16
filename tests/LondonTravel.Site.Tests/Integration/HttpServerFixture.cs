// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AspNet.Security.OAuth.Amazon;
using AspNet.Security.OAuth.Apple;
using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A test fixture representing an HTTP server hosting the application. This class cannot be inherited.
/// </summary>
public sealed class HttpServerFixture : TestServerFixture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServerFixture"/> class.
    /// </summary>
    public HttpServerFixture()
        : base()
    {
        Interceptor.RegisterBundle(Path.Combine("Integration", "oauth-http-bundle.json"));
        Interceptor.RegisterBundle(Path.Combine("Integration", "tfl-http-bundle.json"));

        UseKestrel(
            (server) => server.Listen(
                IPAddress.Loopback, 0, (listener) => listener.UseHttps(
                    (https) => https.ServerCertificate = LoadDevelopmentCertificate())));
    }

    /// <summary>
    /// Gets the server address of the application.
    /// </summary>
    public Uri ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress;
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
            p.AddSingleton<IPostConfigureOptions<SiteOptions>>(ResolvePostConfigureOptions);
            p.AddSingleton<IPostConfigureOptions<TwitterOptions>>(ResolvePostConfigureOptions);
        });
    }

    private static X509Certificate2 LoadDevelopmentCertificate()
    {
        var metadata = typeof(HttpServerFixture).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToArray();

        string? fileName = metadata.First((p) => p.Key is "DevCertificateFileName").Value!;
        string? password = metadata.First((p) => p.Key is "DevCertificatePassword").Value;

        return X509CertificateLoader.LoadPkcs12(File.ReadAllBytes(fileName), password);
    }
}
