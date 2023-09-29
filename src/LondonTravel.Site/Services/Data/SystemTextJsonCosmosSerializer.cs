// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.Azure.Cosmos;

namespace MartinCostello.LondonTravel.Site.Services.Data;

internal sealed class SystemTextJsonCosmosSerializer(JsonSerializerOptions options) : CosmosSerializer
{
    /// <inheritdoc />
    public override T FromStream<T>(Stream stream)
    {
        // Have to dispose of the stream, otherwise the Cosmos SDK throws.
        // https://github.com/Azure/azure-cosmos-dotnet-v3/blob/0843cae3c252dd49aa8e392623d7eaaed7eb712b/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonSerializerWrapper.cs#L22
        // https://github.com/Azure/azure-cosmos-dotnet-v3/blob/0843cae3c252dd49aa8e392623d7eaaed7eb712b/Microsoft.Azure.Cosmos/src/Serializer/CosmosJsonDotNetSerializer.cs#L73
        using (stream)
        {
            // TODO Would be more efficient if CosmosSerializer supported async
            // See https://github.com/Azure/azure-cosmos-dotnet-v3/issues/715
            using var memory = new MemoryStream((int)stream.Length);
            stream.CopyTo(memory);

            byte[] utf8Json = memory.ToArray();

            var result = JsonSerializer.Deserialize<T>(utf8Json, options);
            return result!;
        }
    }

    /// <inheritdoc />
    public override Stream ToStream<T>(T input)
    {
        byte[] utf8Json = JsonSerializer.SerializeToUtf8Bytes(input, options);
        return new MemoryStream(utf8Json);
    }
}
