// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using MartinCostello.LondonTravel.Site.Swagger;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an error from an API resource.
    /// </summary>
    [SwaggerTypeExample(typeof(ErrorResponseExampleProvider))]
    public sealed class ErrorResponse
    {
        /// <summary>
        /// Gets or sets the HTTP status code.
        /// </summary>
        [JsonProperty("statusCode")]
        [Required]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the request Id.
        /// </summary>
        [JsonProperty("requestId")]
        [Required]
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the error details, if any.
        /// </summary>
        [JsonProperty("details")]
        [Required]
        public ICollection<string> Details { get; set; }
    }
}
