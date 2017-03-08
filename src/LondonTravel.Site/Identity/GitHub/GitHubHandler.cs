// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.GitHub
{
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.WebUtilities;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A class representing an OAuth handler to use for GitHub.
    /// </summary>
    internal class GitHubHandler : OAuthHandler<GitHubOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitHubHandler"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public GitHubHandler(HttpClient httpClient)
            : base(httpClient)
        {
        }

        /// <inheritdoc />
        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            JObject payload = await GetUserAsync(tokens.AccessToken);
            string email = await GetUserEmailAsync(tokens.AccessToken, Options.PreferPrimaryEmail);

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Options.AuthenticationScheme);
            var context = new OAuthCreatingTicketContext(ticket, Context, Options, Backchannel, tokens, payload);

            string identifier = (string)payload["login"];

            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            if (!string.IsNullOrEmpty(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string name = (string)payload["name"];

            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(identity.NameClaimType, name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }

        private async Task<JObject> GetUserAsync(string accessToken)
        {
            string endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", accessToken);

            using (var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve GitHub user information ({response.StatusCode}).");
                }

                return JObject.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        private async Task<string> GetUserEmailAsync(string accessToken, bool preferPrimary)
        {
            string endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint + "/emails", "access_token", accessToken);

            using (var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve GitHub user email information ({response.StatusCode}).");
                }

                JArray payload = JArray.Parse(await response.Content.ReadAsStringAsync());
                JObject email = null;

                if (preferPrimary)
                {
                    foreach (JObject item in payload)
                    {
                        bool? isPrimary = (bool?)email["primary"];

                        if (isPrimary == true)
                        {
                            email = item;
                            break;
                        }
                    }
                }

                return (string)(email ?? payload.First)?["email"];
            }
        }
    }
}
