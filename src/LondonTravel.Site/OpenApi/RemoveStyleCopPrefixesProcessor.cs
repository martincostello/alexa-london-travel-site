// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NJsonSchema.Generation;

namespace MartinCostello.LondonTravel.Site.OpenApi;

public sealed class RemoveStyleCopPrefixesProcessor : ISchemaProcessor
{
    private const string Prefix = "Gets or sets ";

    /// <inheritdoc/>
    public void Process(SchemaProcessorContext context)
    {
        foreach ((_, var property) in context.Schema.ActualProperties)
        {
            if (property.Description is not null)
            {
                property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);
                property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description[1..];
            }
        }
    }
}
