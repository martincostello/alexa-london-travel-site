// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;

    /// <summary>
    /// Configuration options for <see cref="AmazonHandler"/>.
    /// </summary>
    public class AmazonOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonOptions"/> class.
        /// </summary>
        public AmazonOptions()
        {
            CallbackPath = "/signin-amazon";

            AuthorizationEndpoint = AmazonDefaults.AuthorizationEndpoint;
            TokenEndpoint = AmazonDefaults.TokenEndpoint;
            UserInformationEndpoint = AmazonDefaults.UserInformationEndpoint;

            Scope.Add("profile");
            Scope.Add("profile:user_id");

            Fields.Add("email");
            Fields.Add("name");
            Fields.Add("user_id");

            ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
            ClaimActions.MapJsonKey(ClaimTypes.PostalCode, "postal_code");
        }

        /// <summary>
        /// Gets the list of fields to retrieve from the user information endpoint.
        /// </summary>
        public ICollection<string> Fields { get; } = new HashSet<string>();
    }
}
