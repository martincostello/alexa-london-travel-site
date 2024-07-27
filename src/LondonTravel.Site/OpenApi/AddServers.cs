// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class AddServers(
    IHttpContextAccessor accessor,
    IOptions<ForwardedHeadersOptions> options) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Servers = [new() { Url = GetServerUrl(accessor, options.Value) }];
        return Task.CompletedTask;
    }

    private static string GetServerUrl(IHttpContextAccessor accessor, ForwardedHeadersOptions options)
    {
        var request = accessor.HttpContext!.Request;

        string scheme = TryGetFirstHeader(options.ForwardedProtoHeaderName) ?? request.Scheme;
        string host = TryGetFirstHeader(options.ForwardedHostHeaderName) ?? request.Host.ToString();

        return new Uri($"{scheme}://{host}").ToString().TrimEnd('/');

        string? TryGetFirstHeader(string name)
            => request.Headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
    }
}
