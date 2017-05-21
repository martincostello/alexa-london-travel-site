// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using Services.Data;
    using Telemetry;

    /// <summary>
    /// A class representing the controller for the <c>/api</c> resource.
    /// </summary>
    [Route("api")]
    public class ApiController : Controller
    {
        /// <summary>
        /// The <see cref="IDocumentClient"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentClient _client;

        /// <summary>
        /// The <see cref="ISiteTelemetry"/> to use. This field is read-only.
        /// </summary>
        private readonly ISiteTelemetry _telemetry;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<ApiController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
        /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public ApiController(IDocumentClient client, ISiteTelemetry telemetry, ILogger<ApiController> logger)
        {
            _client = client;
            _telemetry = telemetry;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/api/preferences</c> action.
        /// </summary>
        /// <param name="authorizationHeader">The value of the authorization header.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// The result for the <c>/api/preference</c> action.
        /// </returns>
        [HttpGet]
        [Produces("application/json", Type = typeof(PreferencesResponse))]
        [Route("preferences")]
        public async Task<IActionResult> GetPreferences(
            [FromHeader(Name = "Authorization")] string authorizationHeader,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger?.LogTrace("Received API request for user preferences.");

            // TODO Consider allowing implicit access if the user is signed-in (i.e. access from a browser)
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                _logger?.LogInformation(
                    "API request for preferences denied as no Authorization header/value was specified. IP: {RemoteIP}; User Agent: {UserAgent}.",
                    HttpContext.Connection.RemoteIpAddress,
                    Request.Headers["User-Agent"]);

                _telemetry.TrackApiPreferencesUnauthorized();

                return Unauthorized("No access token specified.");
            }

            string accessToken = GetAccessTokenFromAuthorizationHeader(authorizationHeader);
            LondonTravelUser user = await FindUserByAccessTokenAsync(accessToken, cancellationToken);

            if (user == null || !string.Equals(user.AlexaToken, accessToken, StringComparison.Ordinal))
            {
                _logger?.LogInformation(
                    "API request for preferences denied as the specified access token is unknown. IP: {RemoteIP}; User Agent: {UserAgent}.",
                    HttpContext.Connection.RemoteIpAddress,
                    Request.Headers["User-Agent"]);

                _telemetry.TrackApiPreferencesUnauthorized();

                return Unauthorized("Unauthorized.");
            }

            _logger?.LogInformation(
                "Successfully authorized API request for preferences for user Id {UserId}. IP: {RemoteIP}; User Agent: {UserAgent}.",
                user.Id,
                HttpContext.Connection.RemoteIpAddress,
                Request.Headers["User-Agent"]);

            var data = new PreferencesResponse()
            {
                FavoriteLines = user.FavoriteLines,
                UserId = user.Id,
            };

            _telemetry.TrackApiPreferencesSuccess(data.UserId);

            return Ok(data);
        }

        /// <summary>
        /// Extracts the Alexa access token from the specified Authorize HTTP header value.
        /// </summary>
        /// <param name="authorizationHeader">The raw Authorization HTTP request header value.</param>
        /// <returns>
        /// The Alexa access token extracted from <paramref name="authorizationHeader"/>, if anyl otherwise <see langword="null"/>.
        /// </returns>
        private static string GetAccessTokenFromAuthorizationHeader(string authorizationHeader)
        {
            if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue authorization) ||
                !string.Equals(authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorization.Parameter;
        }

        /// <summary>
        /// Finds the user with the specified access token, if any, as an asynchronous operation.
        /// </summary>
        /// <param name="accessToken">The access token to find the associated user for.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation to
        /// find the London Travel user with the specified Alexa access token.
        /// </returns>
        private async Task<LondonTravelUser> FindUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            LondonTravelUser user = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    user = (await _client.GetAsync<LondonTravelUser>((p) => p.AlexaToken == accessToken, cancellationToken)).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(default(EventId), ex, "Failed to find user by access token.");
                    throw;
                }
            }

            return user;
        }

        /// <summary>
        /// Returns a response to use for an unauthorized API request.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <returns>
        /// The created instance of <see cref="ObjectResult"/>.
        /// </returns>
        private ObjectResult Unauthorized(string message)
        {
            var error = new ErrorResponse()
            {
                Message = message ?? string.Empty,
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = (int)HttpStatusCode.Unauthorized,
            };

            return StatusCode((int)HttpStatusCode.Unauthorized, error);
        }
    }
}
