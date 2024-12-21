// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using Microsoft.Extensions.Logging;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// The base class for browser tests.
/// </summary>
[Collection<HttpServerCollection>]
public abstract class BrowserIntegrationTest : BrowserTest
{
    private bool _disposed;
    private IDisposable? _scope;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowserIntegrationTest"/> class.
    /// </summary>
    /// <param name="fixture">The fixture to use.</param>
    /// <param name="outputHelper">The <see cref="ITestOutputHelper"/> to use.</param>
    protected BrowserIntegrationTest(HttpServerFixture fixture, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Fixture = fixture;
        Fixture.SetOutputHelper(outputHelper);

        var logger = outputHelper.ToLogger<HttpClientInterceptorOptions>();

        Fixture.Interceptor.OnSend = (request) =>
        {
#pragma warning disable CA1848
            logger.LogInformation("HTTP request intercepted. {Request}", request);
            return Task.CompletedTask;
#pragma warning restore CA1848
        };

        _scope = Fixture.Interceptor.BeginScope();
    }

    /// <summary>
    /// Gets the <see cref="HttpServerFixture"/> to use.
    /// </summary>
    protected HttpServerFixture Fixture { get; }

    /// <inheritdoc/>
    protected override Uri ServerAddress => Fixture.ServerAddress;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!_disposed)
        {
            if (disposing)
            {
                Fixture?.ClearOutputHelper();
                _scope?.Dispose();
                _scope = null;
            }

            _disposed = true;
        }
    }
}
