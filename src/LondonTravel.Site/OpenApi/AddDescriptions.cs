// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class AddDescriptions : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var attributes = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<OpenApiResponseAttribute>()
            .ToArray();

        foreach (var attribute in attributes)
        {
            if (operation.Responses.TryGetValue(attribute.HttpStatusCode.ToString(CultureInfo.InvariantCulture), out var response))
            {
                response.Description = attribute.Description;
            }
        }
    }
}
