// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Identity;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class representing the controller for the <c>/alexa</c> resource.
    /// </summary>
    [Route("alexa")]
    public class AlexaController : Controller
    {
        /// <summary>
        /// The <see cref="UserManager{TUser}"/> to use. This field is read-only.
        /// </summary>
        private readonly UserManager<LondonTravelUser> _userManager;

        /// <summary>
        /// The <see cref="AlexaOptions"/> to use. This field is read-only.
        /// </summary>
        private readonly AlexaOptions _options;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<AlexaController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlexaController"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser}"/> to use.</param>
        /// <param name="siteOptions">The current site options.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public AlexaController(
            UserManager<LondonTravelUser> userManager,
            SiteOptions siteOptions,
            ILogger<AlexaController> logger)
        {
            _userManager = userManager;
            _options = siteOptions?.Alexa;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/alexa/authorize/</c> action.
        /// </summary>
        /// <param name="state">The state from the Alexa service.</param>
        /// <param name="clientId">The client Id.</param>
        /// <param name="responseType">The response type.</param>
        /// <param name="scopes">The access scope(s) requested.</param>
        /// <param name="redirectUri">The URL to redirect the user to once linked.</param>
        /// <returns>
        /// The result for the <c>/alexa/authorize/</c> action.
        /// </returns>
        [Authorize]
        [HttpGet]
        [Route("authorize", Name = SiteRoutes.AuthorizeAlexa)]
        public async Task<IActionResult> AuthorizeSkill(
            [FromQuery(Name = "state")] string state,
            [FromQuery(Name = "client_id")] string clientId,
            [FromQuery(Name = "response_type")] string responseType,
            [FromQuery(Name = "scope")] ICollection<string> scopes,
            [FromQuery(Name = "redirect_uri")] Uri redirectUri)
        {
            if (_options?.IsLinkingEnabled != true)
            {
                return NotFound();
            }

            if (!VerifyClientId(clientId) || !VerifyResponseType(responseType) || !VerifyRedirectUri(redirectUri))
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogError($"Failed to get user to link account to Alexa.");
                return View("Error", 500);
            }

            string accessToken = GenerateAccessToken();

            if (!await CreateOrUpdateAccessToken(user, accessToken))
            {
                return View("Error", 500);
            }

            string tokenRedirectUrl = BuildRedirectUrl(redirectUri, state, accessToken, responseType);
            return Redirect(tokenRedirectUrl);
        }

        /// <summary>
        /// Generates a new random access token.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the generated access token.
        /// </returns>
        public static string GenerateAccessToken()
        {
            byte[] entropy = new byte[64];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(entropy);
            }

            return Convert.ToBase64String(entropy);
        }

        /// <summary>
        /// Builds the redirection URI for the specified parameters.
        /// </summary>
        /// <param name="redirectUri">The base redirection URI.</param>
        /// <param name="state">The value of the state parameter.</param>
        /// <param name="accessToken">The value of the generated access token.</param>
        /// <param name="responseType">The response type.</param>
        /// <returns>
        /// The URI to redirect the user to receive the generated access token.
        /// </returns>
        private static string BuildRedirectUrl(Uri redirectUri, string state, string accessToken, string responseType)
        {
            UriBuilder builder = new UriBuilder(redirectUri)
            {
                Query = $"state={(state == null ? string.Empty : Uri.EscapeDataString(state))}&access_token={Uri.EscapeDataString(accessToken)}&token_type={Uri.EscapeDataString(responseType)}"
            };

            return builder.Uri.AbsoluteUri.ToString();
        }

        /// <summary>
        /// Either creates a new access token or updates the existing access token for
        /// the specified user as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create or update the access token for.</param>
        /// <param name="accessToken">The new access token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to create
        /// or update the access token for the specified user that returns <see langword="true"/>
        /// if the access token was successfully created/updated; otherwise <see langword="false"/>.
        /// </returns>
        private async Task<bool> CreateOrUpdateAccessToken(LondonTravelUser user, string accessToken)
        {
            bool hasExistingToken = string.IsNullOrEmpty(user.AlexaToken);

            if (hasExistingToken)
            {
                _logger.LogInformation($"Regenerating Alexa acccess token for user '{user.Id}'.");
            }
            else
            {
                _logger.LogInformation($"Generating Alexa acccess token for user '{user.Id}'.");
            }

            user.AlexaToken = accessToken;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (hasExistingToken)
                {
                    _logger.LogInformation($"Regenerated Alexa acccess token for user '{user.Id}'.");
                }
                else
                {
                    _logger.LogInformation($"Generated Alexa acccess token for user '{user.Id}'.");
                }
            }
            else
            {
                _logger.LogError(
                    $"Failed to generate Alexa access token for user '{user.Id}': {string.Join(";", result.Errors.Select((p) => $"{p.Code}: {p.Description}"))}.");
            }

            return result.Succeeded;
        }

        /// <summary>
        /// Verifies the client Id.
        /// </summary>
        /// <param name="clientId">The client Id to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the specified client Id is valid; otherwise <see langword="false"/>.
        /// </returns>
        private bool VerifyClientId(string clientId)
        {
            if (!string.Equals(clientId, _options.ClientId, StringComparison.Ordinal))
            {
                _logger.LogError($"Invalid client Id '{clientId}' specified.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies the redirection URI.
        /// </summary>
        /// <param name="redirectUri">The client Id to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the specified client Id is valid; otherwise <see langword="false"/>.
        /// </returns>
        private bool VerifyRedirectUri(Uri redirectUri)
        {
            if (redirectUri == null)
            {
                _logger.LogWarning($"No redirection URI '{redirectUri}' specified.");
                return false;
            }

            if (!redirectUri.IsAbsoluteUri)
            {
                _logger.LogWarning($"The specified redirection URI '{redirectUri}' is not an absolute URI.");
                return false;
            }

            if (_options?.RedirectUrls.Contains(redirectUri.ToString(), StringComparer.OrdinalIgnoreCase) == false)
            {
                _logger.LogWarning($"The specified redirection URI '{redirectUri}' is an authorized redirection URI.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies the response type.
        /// </summary>
        /// <param name="responseType">The response type to verify.</param>
        /// <returns>
        /// <see langword="true"/> if the specified response type is valid; otherwise <see langword="false"/>.
        /// </returns>
        private bool VerifyResponseType(string responseType)
        {
            const string ImplicitFlowResponseType = "token";

            if (!string.Equals(responseType, ImplicitFlowResponseType, StringComparison.Ordinal))
            {
                _logger.LogError($"Invalid response type '{responseType}' specified.");
                return false;
            }

            return true;
        }
    }
}
