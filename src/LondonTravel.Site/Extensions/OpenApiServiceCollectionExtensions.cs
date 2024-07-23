// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.OpenApi;

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
        value.AddScoped<AddExamplesTransformer>();

        return value.AddOpenApi("api", (options) =>
        {
            options.AddDocumentTransformer<AddApiInfoTransformer>();
            options.AddDocumentTransformer<AddSecurityTransformer>();
            options.AddDocumentTransformer<AddServersTransformer>();

            options.AddOperationTransformer(new AddResponseDescriptionTransformer());
            options.AddSchemaTransformer(new AddSchemaDescriptionsTransformer());

            var examples = new AddExamplesTransformer();
            options.AddOperationTransformer(examples);
            options.AddSchemaTransformer(examples);

            var prefixes = new RemoveStyleCopPrefixesTransformer();
            options.AddOperationTransformer(prefixes);
            options.AddSchemaTransformer(prefixes);
        });
    }
}
