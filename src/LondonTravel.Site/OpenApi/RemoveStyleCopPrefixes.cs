// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class RemoveStyleCopPrefixes : IOpenApiSchemaTransformer
{
    private const string Prefix = "Gets or sets ";

    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Description is { } description)
        {
            schema.Description = description.Replace("`", string.Empty, StringComparison.Ordinal);
        }

        foreach (var property in schema.Properties.Values.Where((p) => p.Description is not null))
        {
            property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
            property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
        }

        return Task.CompletedTask;
    }
}
