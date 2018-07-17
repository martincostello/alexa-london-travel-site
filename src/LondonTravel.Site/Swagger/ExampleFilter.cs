// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Swagger
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Swagger;
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
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation != null && context?.ApiDescription != null && context.SchemaRegistry != null)
            {
                var responseAttributes = context.MethodInfo
                    .GetCustomAttributes<SwaggerResponseExampleAttribute>(true)
                    .ToList();

                foreach (var attribute in responseAttributes)
                {
                    var schema = context.SchemaRegistry.GetOrRegister(attribute.ResponseType);

                    var response = operation.Responses
                        .Where((p) => p.Value.Schema?.Type == schema.Type)
                        .Where((p) => p.Value.Schema?.Ref == schema.Ref)
                        .Select((p) => p.Value)
                        .FirstOrDefault();

                    if (response != null)
                    {
                        response.Examples = CreateExample(attribute.ExampleType);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (context.JsonContract != null)
            {
                var attribute = context.SystemType.GetCustomAttribute<SwaggerTypeExampleAttribute>();

                if (attribute != null)
                {
                    model.Example = CreateExample(attribute.ExampleType);
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
        private object CreateExample(Type exampleType)
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
        private object FormatAsJson(IExampleProvider provider)
        {
            var examples = provider.GetExample();

            // Apply any formatting rules configured for the API (e.g. camel casing)
            var json = JsonConvert.SerializeObject(examples, _settings);
            return JsonConvert.DeserializeObject(json);
        }
    }
}
