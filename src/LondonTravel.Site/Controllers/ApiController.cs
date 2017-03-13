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
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<ApiController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiController"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public ApiController(IDocumentClient client, ILogger<ApiController> logger)
        {
            _client = client;
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
            // TODO Consider allowing implicit access if the user is signed-in (i.e. access from a browser)
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return Unauthorized("No access token specified.");
            }

            LondonTravelUser user = null;

            if (AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue authorization) ||
                string.Equals(authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase))
            {
                user = (await _client.GetAsync<LondonTravelUser>((p) => p.AlexaToken == authorization.Parameter, cancellationToken)).FirstOrDefault();
            }

            if (!string.Equals(user?.AlexaToken, authorization.Parameter, StringComparison.Ordinal))
            {
                return Unauthorized("Unauthorized.");
            }

            var data = new PreferencesResponse()
            {
                FavoriteLines = user.FavoriteLines,
                UserId = user.Id,
            };

            return Json(data);
        }

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
