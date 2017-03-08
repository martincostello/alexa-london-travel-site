// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Configuration options for <see cref="AmazonMiddleware"/>.
    /// </summary>
    public class AmazonOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonOptions"/> class.
        /// </summary>
        public AmazonOptions()
        {
            AuthenticationScheme = AmazonDefaults.AuthenticationScheme;
            AuthorizationEndpoint = AmazonDefaults.AuthorizationEndpoint;
            TokenEndpoint = AmazonDefaults.TokenEndpoint;
            UserInformationEndpoint = AmazonDefaults.UserInformationEndpoint;

            CallbackPath = new PathString("/signin-amazon");
            DisplayName = AuthenticationScheme;

            Scope.Add("profile");
            Scope.Add("profile:user_id");

            Fields.Add("email");
            Fields.Add("name");
            Fields.Add("user_id");
        }

        /// <summary>
        /// Gets the list of fields to retrieve from the user information endpoint.
        /// </summary>
        public ICollection<string> Fields { get; } = new HashSet<string>();
    }
}
