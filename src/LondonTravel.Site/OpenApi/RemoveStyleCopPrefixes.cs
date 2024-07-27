// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class RemoveStyleCopPrefixes : IOperationFilter, ISchemaFilter
{
    private const string Prefix = "Gets or sets ";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
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
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Description is not null)
        {
            schema.Description = schema.Description.Replace("`", string.Empty, StringComparison.Ordinal);
        }

        foreach (var property in schema.Properties.Values)
        {
            TryUpdateDescription(property);
        }
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
