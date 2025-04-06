// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Site.Benchmarks;

internal sealed class AppServer : IAsyncDisposable
{
    private WebApplication? _app;
    private Uri? _baseAddress;
    private bool _disposed;

    public AppServer()
    {
        var builder = WebApplication.CreateBuilder(["--applicationName=LondonTravel.Site", $"--contentRoot={GetContentRoot()}"]);

        ConfigureWebHost(builder);

        builder.AddLondonTravelSite();

        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        _app = builder.Build();
        _app.UseLondonTravelSite();
    }

    public HttpClient CreateHttpClient()
    {
#pragma warning disable CA2000
        var handler = new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
        };
#pragma warning restore CA2000

#pragma warning disable CA5400
        return new(handler, disposeHandler: true) { BaseAddress = _baseAddress };
#pragma warning restore CA5400
    }

    public async Task StartAsync()
    {
        if (_app is { } app)
        {
            await app.StartAsync();

            var server = app.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            _baseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();
        }
    }

    public async Task StopAsync()
    {
        if (_app is { } app)
        {
            await app.StopAsync();
            _app = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed && _app is not null)
        {
            await _app.DisposeAsync();
        }

        _disposed = true;
    }

    internal static void ConfigureWebHost(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.WebHost.UseUrls("https://127.0.0.1:0");

        builder.WebHost.UseSetting(
            "ConnectionStrings:AzureCosmos",
            "AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;AccountEndpoint=https://cosmos.local");

        var config = new[]
        {
            KeyValuePair.Create<string, string?>("ConnectionStrings:AzureBlobStorage", string.Empty),
            KeyValuePair.Create<string, string?>("ConnectionStrings:AzureKeyVault", string.Empty),
            KeyValuePair.Create<string, string?>("Site:Authentication:UserStore:CurrentLocation", string.Empty),
        };

        builder.Configuration.AddInMemoryCollection(config);
    }

    private static string? GetRepositoryPath()
    {
        var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(typeof(AppServer).Assembly.Location)!);

        do
        {
            string? solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, "LondonTravel.Site.slnx").FirstOrDefault();

            if (solutionPath is not null)
            {
                return Path.GetDirectoryName(solutionPath);
            }

            directoryInfo = directoryInfo.Parent;
        }
        while (directoryInfo is not null);

        return null;
    }

    private static string GetContentRoot()
    {
        if (GetRepositoryPath() is { } repoPath)
        {
            return Path.GetFullPath(Path.Combine(repoPath, "src", "LondonTravel.Site"));
        }

        return string.Empty;
    }
}
