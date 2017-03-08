// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.GitHub
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Configuration options for <see cref="GitHubMiddleware"/>.
    /// </summary>
    public class GitHubOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubOptions"/> class.
        /// </summary>
        public GitHubOptions()
        {
            AuthenticationScheme = GitHubDefaults.AuthenticationScheme;
            AuthorizationEndpoint = GitHubDefaults.AuthorizationEndpoint;
            TokenEndpoint = GitHubDefaults.TokenEndpoint;
            UserInformationEndpoint = GitHubDefaults.UserInformationEndpoint;

            CallbackPath = new PathString("/signin-github");
            DisplayName = AuthenticationScheme;

            Scope.Add("user:email");
        }

        /// <summary>
        /// Gets or sets a value indicating whether to prefer the
        /// primary email address over the first email address.
        /// </summary>
        public bool PreferPrimaryEmail { get; set; }
    }
}
