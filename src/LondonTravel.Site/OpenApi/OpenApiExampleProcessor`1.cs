// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace MartinCostello.LondonTravel.Site.OpenApi;

public sealed class OpenApiExampleProcessor<T> : IOperationProcessor
    where T : IExampleProvider<T>
{
    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        if (context.Document.Components.Schemas.TryGetValue(typeof(T).Name, out var schema))
        {
            schema.Example = T.GenerateExample();

            foreach (var parameter in context.OperationDescription.Operation.Parameters.Where((p) => p.Schema?.Reference == schema))
            {
                parameter.Example = schema.Example;
            }

            foreach ((_, var response) in context.OperationDescription.Operation.Responses)
            {
                foreach (var mediaType in response.Content.Values.Where((p) => p.Schema?.Reference == schema))
                {
                    mediaType.Example = schema.Example;
                }
            }
        }

        return true;
    }
}
