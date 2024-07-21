// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// Represents an operation processor that adds descriptions to the responses of API endpoints. This class cannot be inherited.
/// </summary>
internal sealed class AddResponseDescriptionTransformer : IOpenApiOperationTransformer
{
    /// <inheritdoc/>
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var attributes = context.Description.ActionDescriptor.EndpointMetadata.OfType<OpenApiResponseAttribute>().ToArray();

        foreach (var attribute in attributes)
        {
            if (operation.Responses.TryGetValue(attribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture), out var response))
            {
                response.Description = attribute.Description;
            }
        }

        return Task.CompletedTask;
    }
}
