// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// A class representing an operation filter that adds the example to use for display in Swagger documentation. This class cannot be inherited.
    /// </summary>
    internal sealed class ExampleFilter : IOperationFilter, ISchemaFilter
    {
        /// <summary>
        /// The <see cref="JsonSerializerSettings"/> to use for formatting example responses. This field is read-only.
        /// </summary>
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleFilter"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="JsonSerializerSettings"/> to use.</param>
        public ExampleFilter(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation != null && context?.ApiDescription != null && context.SchemaRepository != null)
            {
                var responseAttributes = context.MethodInfo
                    .GetCustomAttributes<SwaggerResponseExampleAttribute>(true)
                    .ToList();

                foreach (var attribute in responseAttributes)
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

        /// <inheritdoc />
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.JsonContract != null)
            {
                var attribute = context.Type.GetCustomAttribute<SwaggerTypeExampleAttribute>();

                if (attribute != null)
                {
                    schema.Example = CreateExample(attribute.ExampleType);
                }
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
            var provider = (IExampleProvider)Activator.CreateInstance(exampleType);
            return FormatAsJson(provider);
        }

        /// <summary>
        /// Returns the example from the specified provider formatted as JSON.
        /// </summary>
        /// <param name="provider">The example provider to format the examples for.</param>
        /// <returns>
        /// An <see cref="object"/> representing the formatted example.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="provider"/> is <see langword="null"/>.
        /// </exception>
        private IOpenApiAny FormatAsJson(IExampleProvider provider)
        {
            var examples = provider.GetExample();

            // Apply any formatting rules configured for the API (e.g. camel casing)
            var json = JsonConvert.SerializeObject(examples, _settings);
            var @object = JObject.Parse(json);

            var result = new OpenApiObject();

            // Recursively build up the example from the properties of the JObject
            foreach (var token in @object)
            {
                if (TryParse(token.Value, out var any))
                {
                    result[token.Key] = any;
                }
            }

            return result;
        }

        private bool TryParse(JToken token, out IOpenApiAny any)
        {
            any = null;

            switch (token.Type)
            {
                case JTokenType.Array:
                    var array = new OpenApiArray();

                    foreach (var value in token as JArray)
                    {
                        if (TryParse(value, out var child))
                        {
                            array.Add(child);
                        }
                    }

                    any = array;
                    return true;

                case JTokenType.Boolean:
                    any = new OpenApiBoolean(token.Value<bool>());
                    return true;

                case JTokenType.Date:
                    any = new OpenApiDate(token.Value<DateTime>());
                    return true;

                case JTokenType.Float:
                    any = new OpenApiDouble(token.Value<double>());
                    return true;

                case JTokenType.Guid:
                    any = new OpenApiString(token.Value<Guid>().ToString());
                    return true;

                case JTokenType.Integer:
                    any = new OpenApiInteger(token.Value<int>());
                    return true;

                case JTokenType.String:
                case JTokenType.TimeSpan:
                case JTokenType.Uri:
                    any = new OpenApiString(token.Value<string>());
                    return true;

                case JTokenType.Object:
                    var obj = new OpenApiObject();

                    foreach (var child in token as JObject)
                    {
                        if (TryParse(child.Value, out var value))
                        {
                            obj[child.Key] = value;
                        }
                    }

                    any = obj;
                    return true;

                case JTokenType.Null:
                default:
                    return false;
            }
        }
    }
}
