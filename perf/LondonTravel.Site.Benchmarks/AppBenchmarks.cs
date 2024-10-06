// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace MartinCostello.LondonTravel.Site.Benchmarks;

[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[MemoryDiagnoser]
public class AppBenchmarks : IAsyncDisposable
{
    private AppServer? _app = new();
    private HttpClient? _client;
    private bool _disposed;

    [GlobalSetup]
    public async Task StartServer()
    {
        if (_app is { } app)
        {
            await app.StartAsync();
            _client = app.CreateHttpClient();
        }
    }

    [GlobalCleanup]
    public async Task StopServer()
    {
        if (_app is { } app)
        {
            await app.StopAsync();
            _app = null;
        }
    }

    [Benchmark]
    public async Task<byte[]> Root()
        => await _client!.GetByteArrayAsync("/");

    [Benchmark]
    public async Task<byte[]> Version()
        => await _client!.GetByteArrayAsync("/version");

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        if (!_disposed)
        {
            _client?.Dispose();
            _client = null;

            if (_app is not null)
            {
                await _app.DisposeAsync();
                _app = null;
            }
        }

        _disposed = true;
    }
}
