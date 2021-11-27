// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Provides extension methods for writing a JSON serialized value to the HTTP response.
/// </summary>
/// <remarks>
/// Based on https://github.com/dotnet/aspnetcore/blob/main/src/Http/Http.Extensions/src/HttpResponseJsonExtensions.cs.
/// </remarks>
[ExcludeFromCodeCoverage]
public static class HttpResponseJsonExtensions
{
    private const string JsonContentTypeWithCharset = "application/json; charset=utf-8";

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <typeparam name="TValue">The type of object to write.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync<TValue>(
        this HttpResponse response,
        TValue value,
        JsonSerializerContext? context,
        CancellationToken cancellationToken = default)
    {
        return response.WriteAsJsonAsync(value, context, contentType: null, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// the specified content-type.
    /// </summary>
    /// <typeparam name="TValue">The type of object to write.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync<TValue>(
        this HttpResponse response,
        TValue value,
        JsonSerializerContext? context,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        context ??= ResolveSerializerContext(response.HttpContext);

        response.ContentType = contentType ?? JsonContentTypeWithCharset;

        // If no user-provided token, pass the RequestAborted token and ignore OperationCanceledException
        if (!cancellationToken.CanBeCanceled)
        {
            return WriteAsJsonAsyncSlow(response.Body, value, typeof(TValue), context, response.HttpContext.RequestAborted);
        }

        return JsonSerializer.SerializeAsync(response.Body, value, typeof(TValue), context, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="type">The type of object to write.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync(
        this HttpResponse response,
        object? value,
        Type type,
        CancellationToken cancellationToken = default)
    {
        return response.WriteAsJsonAsync(value, type, context: null, contentType: null, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="type">The type of object to write.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync(
        this HttpResponse response,
        object? value,
        Type type,
        JsonSerializerContext? context,
        CancellationToken cancellationToken = default)
    {
        return response.WriteAsJsonAsync(value, type, context, contentType: null, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// <c>application/json; charset=utf-8</c>.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="jsonTypeInfo">The JSON type metadata to use when serializing the value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync<T>(
        this HttpResponse response,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken = default)
    {
        return response.WriteAsJsonAsync(value, jsonTypeInfo, contentType: null, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// the specified content-type.
    /// </summary>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="type">The type of object to write.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync(
        this HttpResponse response,
        object? value,
        Type type,
        JsonSerializerContext? context,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(type);

        context ??= ResolveSerializerContext(response.HttpContext);

        response.ContentType = contentType ?? JsonContentTypeWithCharset;

        // If no-user provided token, pass the RequestAborted token and ignore OperationCanceledException
        if (!cancellationToken.CanBeCanceled)
        {
            return WriteAsJsonAsyncSlow(response.Body, value, type, context, response.HttpContext.RequestAborted);
        }

        return JsonSerializer.SerializeAsync(response.Body, value, type, context, cancellationToken);
    }

    /// <summary>
    /// Write the specified value as JSON to the response body. The response content-type will be set to
    /// the specified content-type.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="response">The response to write JSON to.</param>
    /// <param name="value">The value to write as JSON.</param>
    /// <param name="jsonTypeInfo">The JSON type metadata to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public static Task WriteAsJsonAsync<T>(
        this HttpResponse response,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.ContentType = contentType ?? JsonContentTypeWithCharset;

        // If no-user provided token, pass the RequestAborted token and ignore OperationCanceledException
        if (!cancellationToken.CanBeCanceled)
        {
            return WriteAsJsonAsyncSlow(response.Body, value, jsonTypeInfo, response.HttpContext.RequestAborted);
        }

        return JsonSerializer.SerializeAsync(response.Body, value, jsonTypeInfo, cancellationToken);
    }

    private static JsonSerializerContext ResolveSerializerContext(HttpContext httpContext)
    {
        return httpContext.RequestServices?.GetRequiredService<JsonSerializerContext>()!;
    }

    private static async Task WriteAsJsonAsyncSlow<T>(
        Stream body,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            await JsonSerializer.SerializeAsync(body, value, jsonTypeInfo, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private static async Task WriteAsJsonAsyncSlow(
        Stream body,
        object? value,
        Type inputType,
        JsonSerializerContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await JsonSerializer.SerializeAsync(body, value, inputType, context, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
