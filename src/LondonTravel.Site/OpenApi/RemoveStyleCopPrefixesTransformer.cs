// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// A class representing a document processor that removes StyleCop
/// prefixes from property descriptions. This class cannot be inherited.
/// </summary>
internal sealed class RemoveStyleCopPrefixesTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private const string Prefix = "Gets or sets ";

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var response in operation.Responses.Values)
        {
            foreach (var model in response.Content.Values)
            {
                foreach (var property in model.Schema.Properties.Values)
                {
                    TryUpdateDescription(property);
                }
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var property in schema.Properties.Values)
        {
            TryUpdateDescription(property);
        }

        return Task.CompletedTask;
    }

    private static void TryUpdateDescription(OpenApiSchema property)
    {
        if (property.Description is not null)
        {
            property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
            property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
        }
    }
}
