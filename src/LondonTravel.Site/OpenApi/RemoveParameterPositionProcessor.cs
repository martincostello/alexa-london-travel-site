// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.LondonTravel.Site.OpenApi;

public sealed class RemoveParameterPositionProcessor : IOperationProcessor
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        foreach ((_, var parameter) in context.Parameters)
        {
            parameter.OriginalName = null;
            parameter.Position = null;
        }

        return true;
    }
}
