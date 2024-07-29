// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.LondonTravel.Site.OpenApi;

internal interface IOpenApiExampleMetadata
{
    Type SchemaType { get; }

    IOpenApiAny GenerateExample(JsonSerializerContext context);
}
