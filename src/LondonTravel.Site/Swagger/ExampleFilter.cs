// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartinCostello.LondonTravel.Site.Swagger;

/// <summary>
/// A class representing an operation filter that adds the example to use for display in Swagger documentation. This class cannot be inherited.
/// </summary>
internal sealed class ExampleFilter : IOperationFilter, ISchemaFilter
{
    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> to use for formatting example responses. This field is read-only.
    /// </summary>
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleFilter"/> class.
    /// </summary>
    /// <param name="options">The <see cref="JsonOptions"/> to use.</param>
    public ExampleFilter(IOptions<JsonOptions> options)
    {
        _options = options.Value.SerializerOptions;
    }

    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation != null && context?.ApiDescription != null && context.SchemaRepository != null)
        {
            AddResponseExamples(operation, context);
        }
    }

    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type != null)
        {
            var attribute = context.Type.GetCustomAttribute<SwaggerTypeExampleAttribute>();

            if (attribute != null)
            {
                schema.Example = CreateExample(attribute.ExampleType);
            }
        }
    }

    /// <summary>
    /// Returns the example from the specified provider formatted as JSON.
    /// </summary>
    /// <param name="examples">The examples to format.</param>
    /// <param name="options">The optional <see cref="JsonSerializerOptions"/> to use.</param>
    /// <returns>
    /// An <see cref="object"/> representing the formatted example.
    /// </returns>
    internal static IOpenApiAny FormatAsJson(object? examples, JsonSerializerOptions? options = null)
    {
        // Apply any formatting rules configured for the API (e.g. camel casing)
        string? json = JsonSerializer.Serialize(examples, options);
        using var document = JsonDocument.Parse(json);

        var result = new OpenApiObject();

        // Recursively build up the example from the properties of the JObject
        foreach (var token in document.RootElement.EnumerateObject())
        {
            if (TryParse(token.Value, out var any))
            {
                result[token.Name] = any;
            }
        }

        return result;
    }

    /// <summary>
    /// Gets all the attributes of the specified type associated with the API description.
    /// </summary>
    /// <typeparam name="T">The type of the attribute(s) to find.</typeparam>
    /// <param name="apiDescription">The API description.</param>
    /// <returns>
    /// An array containing any found attributes of type <typeparamref name="T"/>.
    /// </returns>
    private static T[] GetAttributes<T>(ApiDescription apiDescription)
        where T : Attribute
    {
        IEnumerable<T> attributes = Enumerable.Empty<T>();

        if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
        {
            attributes = attributes.Concat(methodInfo.GetCustomAttributes<T>(inherit: true));
        }

        if (apiDescription.ActionDescriptor is not null)
        {
            attributes = attributes.Concat(apiDescription.ActionDescriptor.EndpointMetadata.OfType<T>());
        }

        return attributes.ToArray();
    }

    /// <summary>
    /// Tries to parse the specified JSON token for an example.
    /// </summary>
    /// <param name="token">The JSON token to parse.</param>
    /// <param name="any">The the token was parsed, the <see cref="IOpenApiAny"/> to use.</param>
    /// <returns>
    /// <see langword="true"/> if parsed successfully; otherwise <see langword="false"/>.
    /// </returns>
    private static bool TryParse(JsonElement token, out IOpenApiAny? any)
    {
        any = null;

        switch (token.ValueKind)
        {
            case JsonValueKind.Array:
                var array = new OpenApiArray();

                foreach (var value in token.EnumerateArray())
                {
                    if (TryParse(value, out var child))
                    {
                        array.Add(child);
                    }
                }

                any = array;
                return true;

            case JsonValueKind.False:
                any = new OpenApiBoolean(false);
                return true;

            case JsonValueKind.True:
                any = new OpenApiBoolean(true);
                return true;

            case JsonValueKind.Number:
                any = new OpenApiDouble(token.GetDouble());
                return true;

            case JsonValueKind.String:
                any = new OpenApiString(token.GetString());
                return true;

            case JsonValueKind.Object:
                var obj = new OpenApiObject();

                foreach (var child in token.EnumerateObject())
                {
                    if (TryParse(child.Value, out var value))
                    {
                        obj[child.Name] = value;
                    }
                }

                any = obj;
                return true;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return false;
        }
    }

    /// <summary>
    /// Creates an example from the specified type.
    /// </summary>
    /// <param name="exampleType">The type to create the example from.</param>
    /// <returns>
    /// The example value.
    /// </returns>
    private IOpenApiAny CreateExample(Type exampleType)
    {
        var provider = Activator.CreateInstance(exampleType) as IExampleProvider;
        object? examples = provider!.GetExample();

        return FormatAsJson(examples, _options);
    }

    /// <summary>
    /// Adds the response examples.
    /// </summary>
    /// <param name="operation">The operation to add the examples for.</param>
    /// <param name="context">The operation context.</param>
    private void AddResponseExamples(OpenApiOperation operation, OperationFilterContext context)
    {
        var examples = GetAttributes<SwaggerResponseExampleAttribute>(context.ApiDescription);

        foreach (var attribute in examples)
        {
            if (!context.SchemaRepository.Schemas.TryGetValue(attribute.ResponseType.Name, out var schema))
            {
                continue;
            }

            var response = operation.Responses
                .SelectMany((p) => p.Value.Content)
                .Where((p) => p.Value.Schema.Reference.Id == attribute.ResponseType.Name)
                .Select((p) => p)
                .FirstOrDefault();

            if (!response.Equals(new KeyValuePair<string, OpenApiMediaType>()))
            {
                response.Value.Example = CreateExample(attribute.ExampleType);
            }
        }
    }
}
