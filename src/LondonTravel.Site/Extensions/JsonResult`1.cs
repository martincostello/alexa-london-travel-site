// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.AspNetCore.Http.Result;

/// <summary>
/// A result which formats the given object as JSON.
/// </summary>
/// <typeparam name="T">
/// The type of the object to format as JSON.
/// </typeparam>
/// <remarks>
/// Based on https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Http/Http.Results/src/JsonResult.cs.
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed partial class JsonResult<T> : IResult
{
    /// <summary>
    /// Gets the <see cref="Net.Http.Headers.MediaTypeHeaderValue"/> representing the Content-Type header of the response.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the JSON type information to use.
    /// </summary>
    public JsonTypeInfo<T> JsonTypeInfo { get; init; } = default!;

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets the value to be formatted.
    /// </summary>
    public T Value { get; init; } = default!;

    /// <inheritdoc />
    Task IResult.ExecuteAsync(HttpContext httpContext)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<JsonResult<T>>>();
        Log.JsonResultExecuting(logger, Value);

        if (StatusCode is int statusCode)
        {
            httpContext.Response.StatusCode = statusCode;
        }

        return httpContext.Response.WriteAsJsonAsync(Value, JsonTypeInfo, ContentType);
    }

    private static partial class Log
    {
        public static void JsonResultExecuting(ILogger logger, object? value)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                string? type = value == null ? "null" : value.GetType().FullName!;
                JsonResultExecuting(logger, type);
            }
        }

        [LoggerMessage(
            1,
            LogLevel.Information,
            "Executing JsonResult, writing value of type '{Type}'.",
            EventName = "JsonResultExecuting",
            SkipEnabledCheck = true)]
        private static partial void JsonResultExecuting(ILogger logger, string type);
    }
}
