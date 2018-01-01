// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Identity;
    using MartinCostello.LondonTravel.Site.Services;
    using MartinCostello.LondonTravel.Site.Swagger;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;
    using Telemetry;

    /// <summary>
    /// A class representing the controller for the <c>/api</c> resource.
    /// </summary>
    [Route("api")]
    public class ApiController : Controller
    {
        /// <summary>
        /// The <see cref="IAccountService"/> to use. This field is read-only.
        /// </summary>
        private readonly IAccountService _service;

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
        /// <param name="service">The <see cref="IAccountService"/> to use.</param>
        /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public ApiController(IAccountService service, ISiteTelemetry telemetry, ILogger<ApiController> logger)
        {
            _service = service;
            _telemetry = telemetry;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/api/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/api/</c> action.
        /// </returns>
        [HttpGet]
        public IActionResult Index() => View();

        /// <summary>
        /// Gets the result for the <c>/api/_count</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/api/_count</c> action.
        /// </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Authorize(Roles = "ADMINISTRATOR")]
        [HttpGet]
        [Produces("application/json")]
        [Route("_count")]
        public async Task<IActionResult> GetDocumentCount()
        {
            long count = await _service.GetUserCountAsync(useCache: false);

            var value = new { count };

            return Ok(value);
        }

        /// <summary>
        /// Gets the preferences for a user associated with an access token.
        /// </summary>
        /// <param name="authorizationHeader">The authorization header.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        /// <returns>
        /// The preferences for a user.
        /// </returns>
        /// <response code="200">The preferences associated with the provided access token.</response>
        /// <response code="401">A valid access token was not provided.</response>
        /// <response code="500">An internal error occurred.</response>
        [HttpGet]
        [Produces("application/json", Type = typeof(PreferencesResponse))]
        [ProducesResponseType(typeof(PreferencesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [Route("preferences")]
        [SwaggerResponseExample(typeof(PreferencesResponse), typeof(PreferencesResponseExampleProvider))]
        public async Task<IActionResult> GetPreferences(
            [FromHeader(Name = "Authorization")] string authorizationHeader,
            CancellationToken cancellationToken = default)
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

            LondonTravelUser user = null;
            string accessToken = GetAccessTokenFromAuthorizationHeader(authorizationHeader, out string errorDetail);

            if (accessToken != null)
            {
                user = await _service.GetUserByAccessTokenAsync(accessToken, cancellationToken);
            }

            if (user == null || !string.Equals(user.AlexaToken, accessToken, StringComparison.Ordinal))
            {
                _logger?.LogInformation(
                    "API request for preferences denied as the specified access token is unknown. IP: {RemoteIP}; User Agent: {UserAgent}.",
                    HttpContext.Connection.RemoteIpAddress,
                    Request.Headers["User-Agent"]);

                _telemetry.TrackApiPreferencesUnauthorized();

                return Unauthorized("Unauthorized.", errorDetail);
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
        /// <param name="errorDetail">When the method returns contains details about an error if the access token is invalid.</param>
        /// <returns>
        /// The Alexa access token extracted from <paramref name="authorizationHeader"/>, if anyl otherwise <see langword="null"/>.
        /// </returns>
        private static string GetAccessTokenFromAuthorizationHeader(string authorizationHeader, out string errorDetail)
        {
            errorDetail = null;

            if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue authorization))
            {
                errorDetail = "The provided authorization value is not valid.";
                return null;
            }

            if (!string.Equals(authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase))
            {
                errorDetail = "Only the bearer authorization scheme is supported.";
                return null;
            }

            return authorization.Parameter;
        }

        /// <summary>
        /// Returns a response to use for an unauthorized API request.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="detail">The optional error detail.</param>
        /// <returns>
        /// The created instance of <see cref="ObjectResult"/>.
        /// </returns>
        private ObjectResult Unauthorized(string message, string detail = null)
        {
            var error = new ErrorResponse()
            {
                Message = message ?? string.Empty,
                RequestId = HttpContext.TraceIdentifier,
                StatusCode = StatusCodes.Status401Unauthorized,
                Details = detail == null ? Array.Empty<string>() : new[] { detail },
            };

            return StatusCode(error.StatusCode, error);
        }
    }
}
