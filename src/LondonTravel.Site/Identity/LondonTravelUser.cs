// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
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
            Logins = new List<LondonTravelLoginInfo>();
        }

        /// <summary>
        /// Gets or sets the user Id.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the normalized email address.
        /// </summary>
        [JsonProperty(PropertyName = "emailNormalized")]
        public string EmailNormalized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email address has been confirmed.
        /// </summary>
        [JsonProperty(PropertyName = "emailConfirmed")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the user's given name.
        /// </summary>
        [JsonProperty(PropertyName = "givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the user's surname.
        /// </summary>
        [JsonProperty(PropertyName = "surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the normalized user name.
        /// </summary>
        [JsonProperty(PropertyName = "userNameNormalized")]
        public string UserNameNormalized { get; set; }

        /// <summary>
        /// Gets or sets the user's external logins.
        /// </summary>
        [JsonProperty(PropertyName = "logins")]
        public IList<LondonTravelLoginInfo> Logins { get; set; }
    }
}
