// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// A class representing a user of the application.
    /// </summary>
    public class LondonTravelUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LondonTravelUser"/> class.
        /// </summary>
        public LondonTravelUser()
        {
            FavoriteLines = new List<string>();
            Logins = new List<LondonTravelLoginInfo>();
            RoleClaims = new List<LondonTravelRole>();
        }

        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the ETag of the underlying document.
        /// </summary>
        [JsonProperty("_etag")]
        public string ETag { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the normalized email address.
        /// </summary>
        [JsonProperty("emailNormalized")]
        public string EmailNormalized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email address has been confirmed.
        /// </summary>
        [JsonProperty("emailConfirmed")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the user's given name.
        /// </summary>
        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the user's surname.
        /// </summary>
        [JsonProperty("surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonProperty("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the normalized user name.
        /// </summary>
        [JsonProperty("userNameNormalized")]
        public string UserNameNormalized { get; set; }

        /// <summary>
        /// Gets or sets the user's external logins.
        /// </summary>
        [JsonProperty("logins")]
        public IList<LondonTravelLoginInfo> Logins { get; set; }

        /// <summary>
        /// Gets or sets the user's role claims.
        /// </summary>
        [JsonProperty("roleClaims")]
        public IList<LondonTravelRole> RoleClaims { get; set; }

        /// <summary>
        /// Gets or sets the user's favorite line Ids.
        /// </summary>
        [JsonProperty("favoriteLines")]
        public IList<string> FavoriteLines { get; set; }

        /// <summary>
        /// Gets or sets the user's Amazon Alexa access token.
        /// </summary>
        [JsonProperty("alexaToken")]
        public string AlexaToken { get; set; }

        /// <summary>
        /// Gets or sets the user's security stamp.
        /// </summary>
        [JsonProperty("securityStamp")]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Gets or sets the date and time the user was created.
        /// </summary>
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the Unix timestamp of the user document.
        /// </summary>
        [JsonProperty("_ts")]
        internal long Timestamp { get; set; }
    }
}
