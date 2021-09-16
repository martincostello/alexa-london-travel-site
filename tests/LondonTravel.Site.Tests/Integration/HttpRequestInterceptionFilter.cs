// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using Microsoft.Extensions.Http;

namespace MartinCostello.LondonTravel.Site.Integration;

/// <summary>
/// A class representing an <see cref="IHttpMessageHandlerBuilderFilter"/> that configures
/// HTTP request interception with HttpClientFactory. This class cannot be inherited.
/// </summary>
/// <remarks>
/// See https://github.com/justeat/httpclient-interception/blob/4e52f0e269654bbcf4745aa307624d807e4f19e2/samples/SampleApp.Tests/HttpServerFixture.cs#L27-L30.
/// </remarks>
internal sealed class HttpRequestInterceptionFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly HttpClientInterceptorOptions _options;

    internal HttpRequestInterceptionFilter(HttpClientInterceptorOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return (builder) =>
        {
            next(builder);
            builder.AdditionalHandlers.Add(_options.CreateHttpMessageHandler());
        };
    }
}
