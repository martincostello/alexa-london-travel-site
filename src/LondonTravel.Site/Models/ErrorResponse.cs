// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// A class representing the error response from an API resource. This class cannot be inherited.
    /// </summary>
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
    }
}
