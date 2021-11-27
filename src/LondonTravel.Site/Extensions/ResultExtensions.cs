// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Http.Result;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// A class containing extensions for <see cref="IResult"/> that use the JSON source generator.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ResultExtensions
{
    /// <summary>
    /// Creates a <see cref="IResult"/> that serializes the specified <paramref name="value"/> object to JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="extensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="value">The object to write as JSON.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <returns>
    /// The created <see cref="JsonResult{Type}"/> that serializes the specified
    /// <paramref name="value"/> as JSON format for the response.</returns>
    /// <remarks>
    /// Callers should cache an instance of serializer settings to avoid recreating cached data with each call.
    /// </remarks>
    public static IResult Json<T>(
        this IResultExtensions extensions,
        T? value,
        JsonSerializerContext? context = null,
        string? contentType = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(extensions);

        return new JsonResult
        {
            ContentType = contentType,
            InputType = typeof(T),
            JsonSerializerContext = context,
            StatusCode = statusCode,
            Value = value,
        };
    }

    /// <summary>
    /// Creates a <see cref="IResult"/> that serializes the specified <paramref name="value"/> object to JSON.
    /// </summary>
    /// <param name="extensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="value">The object to write as JSON.</param>
    /// <param name="inputType">The type of the value to write as JSON.</param>
    /// <param name="context">The serializer context to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <returns>
    /// The created <see cref="JsonResult{Type}"/> that serializes the specified
    /// <paramref name="value"/> as JSON format for the response.</returns>
    /// <remarks>
    /// Callers should cache an instance of serializer settings to avoid recreating cached data with each call.
    /// </remarks>
    public static IResult Json(
        this IResultExtensions extensions,
        object? value,
        Type inputType,
        JsonSerializerContext? context = null,
        string? contentType = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        ArgumentNullException.ThrowIfNull(inputType);

        return new JsonResult
        {
            ContentType = contentType,
            InputType = inputType,
            JsonSerializerContext = context,
            StatusCode = statusCode,
            Value = value,
        };
    }

    /// <summary>
    /// Creates a <see cref="IResult"/> that serializes the specified <paramref name="value"/> object to JSON.
    /// </summary>
    /// <typeparam name="T">The type of the value to write as JSON.</typeparam>
    /// <param name="extensions">The <see cref="IResultExtensions"/> being extended.</param>
    /// <param name="value">The object to write as JSON.</param>
    /// <param name="jsonTypeInfo">The JSON type metadata to use when serializing the value.</param>
    /// <param name="contentType">The content-type to set on the response.</param>
    /// <param name="statusCode">The status code to set on the response.</param>
    /// <returns>
    /// The created <see cref="JsonResult{Type}"/> that serializes the specified
    /// <paramref name="value"/> as JSON format for the response.</returns>
    /// <remarks>
    /// Callers should cache an instance of serializer settings to avoid recreating cached data with each call.
    /// </remarks>
    public static IResult Json<T>(
        this IResultExtensions extensions,
        T value,
        JsonTypeInfo<T> jsonTypeInfo,
        string? contentType = null,
        int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(extensions);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        return new JsonResult<T>
        {
            ContentType = contentType,
            JsonTypeInfo = jsonTypeInfo,
            StatusCode = statusCode,
            Value = value,
        };
    }
}
