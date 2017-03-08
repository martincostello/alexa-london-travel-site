// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    /// <summary>
    /// A class containing defaults for Amazon authentication. This class cannot be inherited.
    /// </summary>
    public static class AmazonDefaults
    {
        /// <summary>
        /// The name of the authentication scheme.
        /// </summary>
        public const string AuthenticationScheme = "Amazon";

        /// <summary>
        /// The URL of the authorization endpoint. This field is read-only.
        /// </summary>
        public static readonly string AuthorizationEndpoint = "https://www.amazon.com/ap/oa";

        /// <summary>
        /// The URL of the token endpoint. This field is read-only.
        /// </summary>
        public static readonly string TokenEndpoint = "https://api.amazon.com/auth/o2/token";

        /// <summary>
        /// The URL of the user information endpoint. This field is read-only.
        /// </summary>
        public static readonly string UserInformationEndpoint = "https://api.amazon.com/user/profile";
    }
}
