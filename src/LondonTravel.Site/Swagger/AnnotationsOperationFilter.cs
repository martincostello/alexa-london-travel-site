// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

//// TODO Remove when https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2215 is available.

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.Swagger;

public class AnnotationsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IEnumerable<object> controllerAttributes = Array.Empty<object>();
        IEnumerable<object> actionAttributes = Array.Empty<object>();
        IEnumerable<object> metadataAttributes = Array.Empty<object>();

        if (context.MethodInfo is not null)
        {
            controllerAttributes = context.MethodInfo.DeclaringType!.GetCustomAttributes(true);
            actionAttributes = context.MethodInfo.GetCustomAttributes(true);
        }

        if (context.ApiDescription.ActionDescriptor.EndpointMetadata is not null)
        {
            metadataAttributes = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        }

        // NOTE: When controller and action attributes are applicable, action attributes should take precendence.
        // Hence why they're at the end of the list (i.e. last one wins).
        // Distinct() is applied due to an ASP.NET Core issue: https://github.com/dotnet/aspnetcore/issues/34199.
        var allAttributes = controllerAttributes
            .Union(actionAttributes)
            .Union(metadataAttributes)
            .Distinct();

        var actionAndEndpointAttribtues = actionAttributes
            .Union(metadataAttributes)
            .Distinct();

        ApplySwaggerOperationAttribute(operation, actionAndEndpointAttribtues);
        ApplySwaggerOperationFilterAttributes(operation, context, allAttributes);
        ApplySwaggerResponseAttributes(operation, context, allAttributes);
    }

    private static void ApplySwaggerOperationAttribute(
        OpenApiOperation operation,
        IEnumerable<object> actionAttributes)
    {
        var swaggerOperationAttribute = actionAttributes
            .OfType<SwaggerOperationAttribute>()
            .FirstOrDefault();

        if (swaggerOperationAttribute == null)
        {
            return;
        }

        if (swaggerOperationAttribute.Summary != null)
        {
            operation.Summary = swaggerOperationAttribute.Summary;
        }

        if (swaggerOperationAttribute.Description != null)
        {
            operation.Description = swaggerOperationAttribute.Description;
        }

        if (swaggerOperationAttribute.OperationId != null)
        {
            operation.OperationId = swaggerOperationAttribute.OperationId;
        }

        if (swaggerOperationAttribute.Tags != null)
        {
            operation.Tags = swaggerOperationAttribute.Tags
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }
    }

    private static void ApplySwaggerOperationFilterAttributes(
        OpenApiOperation operation,
        OperationFilterContext context,
        IEnumerable<object> controllerAndActionAttributes)
    {
        var swaggerOperationFilterAttributes = controllerAndActionAttributes
            .OfType<SwaggerOperationFilterAttribute>();

        foreach (var swaggerOperationFilterAttribute in swaggerOperationFilterAttributes)
        {
            var filter = (IOperationFilter)Activator.CreateInstance(swaggerOperationFilterAttribute.FilterType)!;
            filter.Apply(operation, context);
        }
    }

    private static void ApplySwaggerResponseAttributes(
        OpenApiOperation operation,
        OperationFilterContext context,
        IEnumerable<object> controllerAndActionAttributes)
    {
        var swaggerResponseAttributes = controllerAndActionAttributes.OfType<SwaggerResponseAttribute>();

        foreach (var swaggerResponseAttribute in swaggerResponseAttributes)
        {
            var statusCode = swaggerResponseAttribute.StatusCode.ToString();

            if (operation.Responses == null)
            {
                operation.Responses = new OpenApiResponses();
            }

            if (!operation.Responses.TryGetValue(statusCode, out OpenApiResponse? response))
            {
                response = new OpenApiResponse();
            }

            if (swaggerResponseAttribute.Description != null)
            {
                response.Description = swaggerResponseAttribute.Description;
            }

            operation.Responses[statusCode] = response;

            if (swaggerResponseAttribute.ContentTypes != null)
            {
                response.Content.Clear();

                foreach (var contentType in swaggerResponseAttribute.ContentTypes)
                {
                    var schema = (swaggerResponseAttribute.Type != null && swaggerResponseAttribute.Type != typeof(void))
                        ? context.SchemaGenerator.GenerateSchema(swaggerResponseAttribute.Type, context.SchemaRepository)
                        : null;

                    response.Content.Add(contentType, new OpenApiMediaType { Schema = schema });
                }
            }
        }
    }
}
