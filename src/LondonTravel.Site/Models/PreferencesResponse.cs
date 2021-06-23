// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MartinCostello.LondonTravel.Site.Models
{
    /// <summary>
    /// Represents the API response for a user's preferences.
    /// </summary>
    public sealed class PreferencesResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesResponse"/> class.
        /// </summary>
        public PreferencesResponse()
        {
            FavoriteLines = new List<string>();
            UserId = string.Empty;
        }

        /// <summary>
        /// Gets or sets the Ids of the user's favorite lines, if any.
        /// </summary>
        [JsonPropertyName("favoriteLines")]
        [Required]
        public ICollection<string> FavoriteLines { get; set; }

        /// <summary>
        /// Gets or sets the user's Id.
        /// </summary>
        [JsonPropertyName("userId")]
        [Required]
        public string UserId { get; set; }
    }
}
