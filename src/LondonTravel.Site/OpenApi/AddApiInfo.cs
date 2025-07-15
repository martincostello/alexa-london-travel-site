// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class AddApiInfo(IOptions<SiteOptions> options) : IOpenApiDocumentTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ConfigureInfo(document.Info);

        return Task.CompletedTask;
    }

    private void ConfigureInfo(OpenApiInfo info)
    {
        var siteOptions = options.Value;

        info.Description = siteOptions.Metadata?.Description;
        info.Title = siteOptions.Metadata?.Name;
        info.Version = string.Empty;

        info.Contact = new()
        {
            Name = siteOptions.Metadata?.Author?.Name,
        };

        if (siteOptions.Metadata?.Repository is { } contactUrl)
        {
            info.Contact.Url = new(contactUrl);
        }

        info.License = new()
        {
            Name = "Apache 2.0",
            Url = new("https://www.apache.org/licenses/LICENSE-2.0.html"),
        };

        info.TermsOfService = new UriBuilder()
        {
            Scheme = Uri.UriSchemeHttps,
            Host = siteOptions.Metadata?.Domain!,
            Path = "terms-of-service/",
        }.Uri;
    }
}
