// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag.Annotations;

namespace MartinCostello.LondonTravel.Site.OpenApi;

public sealed class OpenApiExampleAttribute<T>() : OpenApiOperationProcessorAttribute(typeof(OpenApiExampleProcessor<T>))
    where T : IExampleProvider<T>
{
}
