// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Refit;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class representing an implementation of <see cref="IHttpContentSerializer"/> that can be
/// used with the <c>System.Text.Json</c> source generators. This class cannot be inherited.
/// </summary>
internal sealed class SystemTextJsonContentSerializerForSourceGenerator : IHttpContentSerializer
{
    //// Based on https://github.com/reactiveui/refit/blob/main/Refit/SystemTextJsonContentSerializer.cs

    private readonly JsonSerializerContext _jsonSerializerContext;

    public SystemTextJsonContentSerializerForSourceGenerator(JsonSerializerContext jsonSerializerContext)
    {
        _jsonSerializerContext = jsonSerializerContext;
    }

    public HttpContent ToHttpContent<T>(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Use new JsonContent.Create(...) overloads when available in .NET 7,
        // then we don't need a custom JsonContent implementation: https://github.com/dotnet/runtime/issues/51544
        return new JsonContent(item, typeof(T), _jsonSerializerContext);
    }

    public async Task<T?> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        object? result = await JsonSerializer.DeserializeAsync(
            stream,
            typeof(T),
            _jsonSerializerContext,
            cancellationToken).ConfigureAwait(false);

        return (T?)result;
    }

    public string? GetFieldNameForProperty(PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);

        return propertyInfo
            .GetCustomAttributes<JsonPropertyNameAttribute>(true)
            .Select((p) => p.Name)
            .FirstOrDefault();
    }

    /// <summary>
    /// Based on https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http.Json/src/System/Net/Http/Json/JsonContent.cs.
    /// </summary>
    private sealed class JsonContent : HttpContent
    {
        private readonly object _value;
        private readonly Type _objectType;
        private readonly JsonSerializerContext _serializerContext;

        internal JsonContent(object inputValue, Type inputType, JsonSerializerContext context)
        {
            _value = inputValue;
            _objectType = inputType;
            _serializerContext = context;
            Headers.ContentType = new("application/json") { CharSet = "utf-8" };
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return JsonSerializer.SerializeAsync(stream, _value, _objectType, _serializerContext, CancellationToken.None);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
