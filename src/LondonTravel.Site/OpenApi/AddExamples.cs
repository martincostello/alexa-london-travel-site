// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal sealed class AddExamples : IOperationFilter, ISchemaFilter
{
    private static readonly ApplicationJsonSerializerContext Context = ApplicationJsonSerializerContext.Default;

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        Process(operation, context.ApiDescription);
    }

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        Process(schema, context.Type);
    }

    private static void Process(OpenApiOperation operation, ApiDescription description)
    {
        var examples = description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        TryAddResponseExamples(operation.Responses, description, examples);
    }

    private static void Process(OpenApiSchema schema, Type type)
    {
        if (schema.Example is null &&
            GetExampleMetadata(type).FirstOrDefault() is { } metadata)
        {
            schema.Example = metadata.GenerateExample(Context);
        }
    }

    private static void TryAddResponseExamples(
        OpenApiResponses responses,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        foreach (var schemaResponse in description.SupportedResponseTypes)
        {
            schemaResponse.Type ??= schemaResponse.ModelMetadata?.ModelType;

            var metadata = GetExampleMetadata(schemaResponse.Type)?
                .FirstOrDefault((p) => p.SchemaType == schemaResponse.Type);

            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= (metadata ?? examples.SingleOrDefault((p) => p.SchemaType == schemaResponse.Type))?.GenerateExample(Context);
                }
            }
        }
    }

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(Type? type)
        => type?.GetCustomAttributes().OfType<IOpenApiExampleMetadata>() ?? [];
}
