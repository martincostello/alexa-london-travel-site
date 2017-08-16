// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity.Amazon
{
    using System.Net.Http;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A class representing an OAuth handler to use for Amazon.
    /// </summary>
    public class AmazonHandler : OAuthHandler<AmazonOptions>
    {
        public AmazonHandler(IOptionsMonitor<AmazonOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        public Task<AuthenticationTicket> CreateAuthenticationTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            return CreateTicketAsync(identity, properties, tokens);
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

            JObject user;

            using (var response = await Backchannel.GetAsync(endpoint, Context.RequestAborted))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve Amazon user information ({response.StatusCode}).");
                }

                user = JObject.Parse(await response.Content.ReadAsStringAsync());
            }

            var context = new OAuthCreatingTicketContext(
                new ClaimsPrincipal(identity),
                properties,
                Context,
                Scheme,
                Options,
                Backchannel,
                tokens,
                user);

            context.RunClaimActions();

            await Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }
    }
}
