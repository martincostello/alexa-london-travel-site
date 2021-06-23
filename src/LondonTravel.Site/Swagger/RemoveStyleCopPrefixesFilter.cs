// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.Swagger
{
    /// <summary>
    /// A class representing an operation filter that modifies XML documentation that matches <c>StyleCop</c>
    /// requirements to be more human-readable for display in Swagger documentation. This class cannot be inherited.
    /// </summary>
    internal sealed class RemoveStyleCopPrefixesFilter : IOperationFilter
    {
        /// <summary>
        /// The documentation prefix to remove.
        /// </summary>
        private const string Prefix = "Gets or sets ";

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context?.SchemaRepository?.Schemas != null)
            {
                foreach (var definition in context.SchemaRepository.Schemas.Values)
                {
                    if (definition.Properties != null)
                    {
                        foreach (var property in definition.Properties.Values)
                        {
                            if (property.Description != null)
                            {
                                if (property.Description.StartsWith(Prefix, StringComparison.Ordinal))
                                {
                                    // Remove the StyleCop property prefix
                                    property.Description = property.Description.Replace(Prefix, string.Empty, StringComparison.Ordinal);

                                    // Capitalize the first letter that's left over
                                    property.Description = char.ToUpperInvariant(property.Description[0]) + property.Description.Substring(1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
