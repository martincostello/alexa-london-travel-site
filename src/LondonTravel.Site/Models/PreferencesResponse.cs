// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// A class representing the API response for a user's preferences.
    /// </summary>
    public sealed class PreferencesResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesResponse"/> class.
        /// </summary>
        public PreferencesResponse()
        {
            FavoriteLines = new List<string>();
        }

        /// <summary>
        /// Gets or sets the Ids of the user's favorite lines, if any.
        /// </summary>
        [JsonProperty("favoriteLines")]
        [Required]
        public ICollection<string> FavoriteLines { get; set; }

        /// <summary>
        /// Gets or sets the user's Id.
        /// </summary>
        [JsonProperty("userId")]
        [Required]
        public string UserId { get; set; }
    }
}
