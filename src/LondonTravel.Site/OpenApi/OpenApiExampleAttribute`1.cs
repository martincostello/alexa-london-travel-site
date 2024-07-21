// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.OpenApi;

/// <summary>
/// An attribute representing an example for an OpenAPI operation parameter.
/// </summary>
/// <typeparam name="T">The type of the schema.</typeparam>
public sealed class OpenApiExampleAttribute<T>() : OpenApiExampleAttribute<T, T>()
    where T : IExampleProvider<T>
{
}
