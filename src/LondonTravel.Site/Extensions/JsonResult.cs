// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Http.Result;

/// <summary>
/// A result which formats the given object as JSON.
/// </summary>
/// <remarks>
/// Based on https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Http/Http.Results/src/JsonResult.cs.
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed partial class JsonResult : IResult
{
    /// <summary>
    /// Gets the <see cref="Net.Http.Headers.MediaTypeHeaderValue"/> representing the Content-Type header of the response.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the type of the input value to be formatted.
    /// </summary>
    public Type InputType { get; init; } = default!;

    /// <summary>
    /// Gets the serializer context.
    /// </summary>
    public JsonSerializerContext? JsonSerializerContext { get; init; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// Gets the value to be formatted.
    /// </summary>
    public object? Value { get; init; }

    /// <inheritdoc />
    Task IResult.ExecuteAsync(HttpContext httpContext)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<JsonResult>>();
        Log.JsonResultExecuting(logger, Value);

        if (StatusCode is int statusCode)
        {
            httpContext.Response.StatusCode = statusCode;
        }

        return httpContext.Response.WriteAsJsonAsync(Value, InputType, JsonSerializerContext, ContentType);
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
