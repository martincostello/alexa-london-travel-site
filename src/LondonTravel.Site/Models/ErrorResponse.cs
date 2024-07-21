// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MartinCostello.LondonTravel.Site.OpenApi;
using Newtonsoft.Json;

namespace MartinCostello.LondonTravel.Site.Models;

/// <summary>
/// Represents an error from an API resource.
/// </summary>
[OpenApiExample<ErrorResponse>]
public sealed class ErrorResponse : IExampleProvider<ErrorResponse>
{
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    [JsonProperty("statusCode")]
    [JsonPropertyName("statusCode")]
    [Required]
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    [JsonProperty("message")]
    [JsonPropertyName("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the request Id.
    /// </summary>
    [JsonProperty("requestId")]
    [JsonPropertyName("requestId")]
    [Required]
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error details, if any.
    /// </summary>
    [JsonProperty("details")]
    [JsonPropertyName("details")]
    [Required]
    public ICollection<string> Details { get; set; } = [];

    /// <inheritdoc/>
    static ErrorResponse IExampleProvider<ErrorResponse>.GenerateExample()
    {
        return new()
        {
            Message = "Unauthorized.",
            RequestId = "0HKT0TM6UJASI",
            StatusCode = 401,
            Details = ["Only the Bearer authorization scheme is supported."],
        };
    }
}
