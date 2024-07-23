// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// A class representing an operation processor that adds examples to API endpoints. This class cannot be inherited.
/// </summary>
internal sealed class AddExamplesTransformer : IOpenApiOperationTransformer, IOpenApiSchemaTransformer
{
    private static readonly ApplicationJsonSerializerContext Context = ApplicationJsonSerializerContext.Default;

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var examples = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IOpenApiExampleMetadata>()
            .ToArray();

        if (examples.Length > 0)
        {
            TryAddResponseExamples(operation.Responses, context, examples);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.JsonTypeInfo.Type.GetCustomAttributes(false)
            .OfType<IOpenApiExampleMetadata>()
            .FirstOrDefault();

        if (metadata?.GenerateExample(Context) is { } value)
        {
            schema.Example = value;
        }

        return Task.CompletedTask;
    }

    private static void TryAddResponseExamples(
        OpenApiResponses responses,
        OpenApiOperationTransformerContext context,
        IList<IOpenApiExampleMetadata> examples)
    {
        var schemaResponses = context.Description.SupportedResponseTypes
            .Where((p) => examples.Any((r) => r.SchemaType == p.Type))
            .ToArray();

        if (schemaResponses.Length < 1)
        {
            return;
        }

        foreach (var schemaResponse in schemaResponses)
        {
            foreach (var responseFormat in schemaResponse.ApiResponseFormats)
            {
                if (responses.TryGetValue(schemaResponse.StatusCode.ToString(CultureInfo.InvariantCulture), out var response) &&
                    response.Content.TryGetValue(responseFormat.MediaType, out var mediaType))
                {
                    mediaType.Example ??= examples.SingleOrDefault((p) => p.SchemaType == schemaResponse.Type)?.GenerateExample(Context);
                }
            }
        }
    }
}
