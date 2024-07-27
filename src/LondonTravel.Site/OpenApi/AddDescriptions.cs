// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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

        if (operation.Parameters is { Count: > 0 })
        {
            TryAddParameterDescriptions(operation.Parameters, context.ApiDescription);
        }
    }

    private static void TryAddParameterDescriptions(
        IList<OpenApiParameter> parameters,
        ApiDescription description)
    {
        var arguments = description.ActionDescriptor.EndpointMetadata
            .OfType<MethodInfo>()
            .FirstOrDefault()?
            .GetParameters()
            .ToArray();

        if (arguments is { Length: > 0 })
        {
            foreach (var argument in arguments)
            {
                var attribute = argument
                    .GetCustomAttributes<DescriptionAttribute>()
                    .FirstOrDefault();

                if (attribute?.Description is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Description ??= value;
                    }
                }
            }
        }
    }
}
