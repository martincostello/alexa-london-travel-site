// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.OpenApi;
using MartinCostello.OpenApi;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing OpenAPI-related extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI to the services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>
    /// The value specified by <paramref name="services"/>.
    /// </returns>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        const string DocumentName = "api";

        services.AddOpenApi(DocumentName, (options) =>
        {
            options.AddDocumentTransformer<AddApiInfo>();
            options.AddDocumentTransformer<AddSecurity>();
        });

        services.AddOpenApiExtensions(DocumentName, (options) =>
        {
            options.AddExamples = true;
            options.AddServerUrls = true;
            options.DefaultServerUrl = "https://londontravel.martincostello.com";
            options.SerializationContexts.Add(ApplicationJsonSerializerContext.Default);
        });

        return services;
    }
}
