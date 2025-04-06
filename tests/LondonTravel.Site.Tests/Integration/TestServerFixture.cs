// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

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

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class representing a factory for creating instances of the application.
/// </summary>
public class TestServerFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
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
        builder.UseSetting(
            "ConnectionStrings:AzureCosmos",
            "AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;AccountEndpoint=https://cosmos.local");

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
                        options.PrivateKey = async (keyId, cancellationToken) =>
                        {
                            string privateKey = await File.ReadAllTextAsync(
                                Path.Combine("Integration", "apple-test-cert.p8"),
                                cancellationToken);

                            return privateKey.AsMemory();
                        };
                    });
            });

        builder.ConfigureAppConfiguration(ConfigureTests)
               .ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this))
               .UseSolutionRelativeContentRoot(Path.Combine("src", "LondonTravel.Site"), "*.slnx");
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        catch (ObjectDisposedException)
        {
            // HACK Workaround double dispose issue.
            // See https://github.com/dotnet/aspnetcore/pull/37631.
        }
    }

    private void ConfigureTests(IConfigurationBuilder builder)
    {
        string? directory = Path.GetDirectoryName(typeof(TestServerFixture).Assembly.Location);
        string fullPath = Path.Combine(directory ?? ".", "testsettings.json");

        builder.AddJsonFile(fullPath);
    }
}
