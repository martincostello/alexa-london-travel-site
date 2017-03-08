// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
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
    /// A class representing an OAuth handler to use for Amazon.
    /// </summary>
    internal class AmazonHandler : OAuthHandler<AmazonOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonHandler"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public AmazonHandler(HttpClient httpClient)
            : base(httpClient)
        {
        }

        /// <inheritdoc />
        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            string endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken);

            if (Options.Fields.Count > 0)
            {
                endpoint = QueryHelpers.AddQueryString(endpoint, "fields", string.Join(",", Options.Fields));
            }

            JObject payload;

            using (var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve Amazon user information ({response.StatusCode}).");
                }

                payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            }

            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Options.AuthenticationScheme);
            var context = new OAuthCreatingTicketContext(ticket, Context, Options, Backchannel, tokens, payload);

            string identifier = (string)payload["user_id"];

            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string email = (string)payload["email"];

            if (!string.IsNullOrEmpty(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string name = (string)payload["name"];

            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(identity.NameClaimType, name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string postalCode = (string)payload["postal_code"];

            if (!string.IsNullOrEmpty(postalCode))
            {
                identity.AddClaim(new Claim(ClaimTypes.PostalCode, postalCode, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }
    }
}
