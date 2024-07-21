// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// Defines an OpenAPI example.
/// </summary>
public interface IOpenApiExampleMetadata
{
    /// <summary>
    /// Gets the type of the schema associated with the example.
    /// </summary>
    Type SchemaType { get; }

    /// <summary>
    /// Generates an example for the schema.
    /// </summary>
    /// <param name="context">The JSON serializer context to use.</param>
    /// <returns>
    /// The example to use.
    /// </returns>
    IOpenApiAny GenerateExample(JsonSerializerContext context);
}
