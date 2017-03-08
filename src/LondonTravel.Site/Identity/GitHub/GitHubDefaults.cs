// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.GitHub
{
    /// <summary>
    /// A class containing defaults for GitHub authentication. This class cannot be inherited.
    /// </summary>
    public static class GitHubDefaults
    {
        /// <summary>
        /// The name of the authentication scheme.
        /// </summary>
        public const string AuthenticationScheme = "GitHub";

        /// <summary>
        /// The URL of the authorization endpoint. This field is read-only.
        /// </summary>
        public static readonly string AuthorizationEndpoint = "https://github.com/login/oauth/authorize";

        /// <summary>
        /// The URL of the token endpoint. This field is read-only.
        /// </summary>
        public static readonly string TokenEndpoint = "https://github.com/login/oauth/access_token";

        /// <summary>
        /// The URL of the user information endpoint. This field is read-only.
        /// </summary>
        public static readonly string UserInformationEndpoint = "https://api.github.com/user";
    }
}
