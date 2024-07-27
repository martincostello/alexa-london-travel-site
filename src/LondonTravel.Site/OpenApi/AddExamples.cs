// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        if (operation.Parameters is { Count: > 0 } parameters)
        {
            TryAddParameterExamples(parameters, description, examples);
        }

        if (operation.RequestBody is { } body)
        {
            TryAddRequestExamples(body, description, examples);
        }

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

    private static void TryAddParameterExamples(
        IList<OpenApiParameter> parameters,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
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
                var metadata =
                    GetExampleMetadata(argument).FirstOrDefault((p) => p.SchemaType == argument.ParameterType) ??
                    GetExampleMetadata(argument.ParameterType).FirstOrDefault((p) => p.SchemaType == argument.ParameterType) ??
                    examples.FirstOrDefault((p) => p.SchemaType == argument.ParameterType);

                if (metadata?.GenerateExample(Context) is { } value)
                {
                    var parameter = parameters.FirstOrDefault((p) => p.Name == argument.Name);
                    if (parameter is not null)
                    {
                        parameter.Example ??= value;
                    }
                }
            }
        }
    }

    private static void TryAddRequestExamples(
        OpenApiRequestBody body,
        ApiDescription description,
        IList<IOpenApiExampleMetadata> examples)
    {
        if (!body.Content.TryGetValue("application/json", out var mediaType) || mediaType.Example is not null)
        {
            return;
        }

        var bodyParameter = description.ParameterDescriptions.Single((p) => p.Source == BindingSource.Body);

        var metadata =
            GetExampleMetadata(bodyParameter.Type).FirstOrDefault() ??
            examples.FirstOrDefault((p) => p.SchemaType == bodyParameter.Type);

        if (metadata is not null)
        {
            mediaType.Example ??= metadata.GenerateExample(Context);
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

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(ParameterInfo parameter)
        => parameter.GetCustomAttributes().OfType<IOpenApiExampleMetadata>();

    private static IEnumerable<IOpenApiExampleMetadata> GetExampleMetadata(Type? type)
        => type?.GetCustomAttributes().OfType<IOpenApiExampleMetadata>() ?? [];
}
