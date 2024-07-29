// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;

namespace MartinCostello.LondonTravel.Site.OpenApi;

#pragma warning disable CA1813

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
internal class OpenApiExampleAttribute<TSchema, TProvider> : Attribute, IOpenApiExampleMetadata
    where TProvider : IExampleProvider<TSchema>
{
    public Type SchemaType { get; } = typeof(TSchema);

    public virtual TSchema GenerateExample() => TProvider.GenerateExample();

    IOpenApiAny IOpenApiExampleMetadata.GenerateExample(JsonSerializerContext context)
        => ExampleFormatter.AsJson(GenerateExample(), context);
}
