// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.Extensions.Caching.Memory;

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

public sealed class TflServiceTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly HttpClientInterceptorOptions _interceptor = new() { ThrowOnMissingRegistration = true };
    private readonly TflOptions _options = CreateOptions();

    public void Dispose() => _cache?.Dispose();

    [Fact]
    public async Task Can_Get_Line_Information_If_Response_Can_Be_Cached()
    {
        // Arrange
        var builder = CreateBuilder()
            .Requests()
            .ForPath("Line/Mode/dlr%2Celizabeth-line%2Coverground%2Ctube")
            .Responds()
            .WithResponseHeader("Cache-Control", "max-age=3600")
            .WithJsonContent(new[] { new { id = "waterloo-city", name = "Waterloo & City" } });

        _interceptor.Register(builder);

        using var httpClient = _interceptor.CreateHttpClient();
        httpClient.BaseAddress = _options.BaseUri;

        var target = new TflService(httpClient, _cache, _options);

        // Act
        var actual1 = await target.GetLinesAsync();
        var actual2 = await target.GetLinesAsync();

        // Assert
        Assert.NotNull(actual1);
        Assert.Single(actual1);

        var item = actual1.First();

        Assert.Equal("waterloo-city", item.Id);
        Assert.Equal("Waterloo & City", item.Name);

        Assert.Same(actual1, actual2);
    }

    [Fact]
    public async Task Can_Get_Line_Information_If_Response_Cannot_Be_Cached()
    {
        // Arrange
        var builder = CreateBuilder()
            .Requests()
            .ForPath("Line/Mode/dlr%2Celizabeth-line%2Coverground%2Ctube")
            .Responds()
            .WithJsonContent(new[] { new { id = "district", name = "District" } });

        _interceptor.Register(builder);

        using var httpClient = _interceptor.CreateHttpClient();
        httpClient.BaseAddress = _options.BaseUri;

        var target = new TflService(httpClient, _cache, _options);

        // Act
        var actual1 = await target.GetLinesAsync();
        var actual2 = await target.GetLinesAsync();

        // Assert
        Assert.NotNull(actual1);
        Assert.Single(actual1);

        var item = actual1.First();

        Assert.Equal("district", item.Id);
        Assert.Equal("District", item.Name);

        Assert.NotSame(actual1, actual2);
    }

    [Fact]
    public async Task Can_Get_Stop_Points_If_Response_Can_Be_Cached()
    {
        // Arrange
        var builder = CreateBuilder()
            .Requests()
            .ForPath("Line/victoria/StopPoints")
            .Responds()
            .WithResponseHeader("Cache-Control", "max-age=3600")
            .WithJsonContent(new[] { new { id = "940GZZLUOXC", commonName = "Oxford Circus Underground Station", lat = 51.515224, lon = -0.141903 } });

        _interceptor.Register(builder);

        using var httpClient = _interceptor.CreateHttpClient();
        httpClient.BaseAddress = _options.BaseUri;

        var target = new TflService(httpClient, _cache, _options);

        // Act
        var actual1 = await target.GetStopPointsByLineAsync("victoria");
        var actual2 = await target.GetStopPointsByLineAsync("victoria");

        // Assert
        Assert.NotNull(actual1);
        Assert.Single(actual1);

        var item = actual1.First();

        Assert.Equal("940GZZLUOXC", item.Id);
        Assert.Equal("Oxford Circus Underground Station", item.Name);
        Assert.Equal(51.515224, item.Latitude);
        Assert.Equal(-0.141903, item.Longitude);

        Assert.Same(actual1, actual2);
    }

    [Fact]
    public async Task Can_Get_Stop_Points_If_Response_Cannot_Be_Cached()
    {
        // Arrange
        var builder = CreateBuilder()
            .Requests()
            .ForPath("Line/victoria/StopPoints")
            .Responds()
            .WithJsonContent(new[] { new { id = "940GZZLUGPK", commonName = "Green Park Underground Station", lat = 51.506947, lon = -0.142787 } });

        _interceptor.Register(builder);

        using var httpClient = _interceptor.CreateHttpClient();
        httpClient.BaseAddress = _options.BaseUri;

        var target = new TflService(httpClient, _cache, _options);

        // Act
        var actual1 = await target.GetStopPointsByLineAsync("victoria");
        var actual2 = await target.GetStopPointsByLineAsync("victoria");

        // Assert
        Assert.NotNull(actual1);
        Assert.Single(actual1);

        var item = actual1.First();

        Assert.Equal("940GZZLUGPK", item.Id);
        Assert.Equal("Green Park Underground Station", item.Name);
        Assert.Equal(51.506947, item.Latitude);
        Assert.Equal(-0.142787, item.Longitude);

        Assert.NotSame(actual1, actual2);
    }

    private static HttpRequestInterceptionBuilder CreateBuilder()
    {
        return new HttpRequestInterceptionBuilder()
            .ForHttps()
            .ForHost("api.tfl.gov.uk")
            .ForQuery("app_id=My-App-Id&app_key=My-App-Key");
    }

    private static TflOptions CreateOptions()
    {
        return new TflOptions()
        {
            AppId = "My-App-Id",
            AppKey = "My-App-Key",
            BaseUri = new Uri("https://api.tfl.gov.uk/"),
            SupportedModes = ["dlr", "elizabeth-line", "overground", "tube"],
        };
    }
}
