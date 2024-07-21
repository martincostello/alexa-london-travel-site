// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.OpenApi;
using MartinCostello.LondonTravel.Site.Options;
using NSwag;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing OpenAPI-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI to the services.
    /// </summary>
    /// <param name="value">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>
    /// The value specified by <paramref name="value"/>.
    /// </returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection value)
    {
        return value.AddOpenApiDocument((options, provider) =>
        {
            var siteOptions = provider.GetRequiredService<SiteOptions>();

            var terms = new UriBuilder()
            {
                Scheme = Uri.UriSchemeHttps,
                Host = siteOptions.Metadata?.Domain!,
                Path = "terms-of-service/",
            };

            options.DocumentName = "api";
            options.Title = siteOptions.Metadata?.Name;
            options.Version = string.Empty;

            options.PostProcess = (document) =>
            {
                document.Generator = null;

                document.Info.Contact = new()
                {
                    Name = siteOptions.Metadata?.Author?.Name,
                    Url = siteOptions.Metadata?.Repository,
                };

                document.Info.Description = siteOptions.Metadata?.Description;

                document.Info.License = new()
                {
                    Name = "Apache 2.0",
                    Url = "https://www.apache.org/licenses/LICENSE-2.0.html",
                };

                document.Info.TermsOfService = terms.Uri.ToString();
                document.Info.Version = string.Empty;

                string schemeName = "Bearer";

                document.Security.Add(new() { [schemeName] = [] });
                document.SecurityDefinitions.Add(schemeName, new()
                {
                    BearerFormat = "Opaque token",
                    Description = "Access token authentication using a bearer token.",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Type = OpenApiSecuritySchemeType.Http,
                    Scheme = "bearer",
                });
            };

            options.OperationProcessors.Add(new RemoveParameterPositionProcessor());
            options.SchemaSettings.SchemaProcessors.Add(new RemoveStyleCopPrefixesProcessor());
        });
    }
}
