// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Cryptography;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site.Controllers;

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
    /// The <see cref="ISiteTelemetry"/> to use. This field is read-only.
    /// </summary>
    private readonly ISiteTelemetry _telemetry;

    /// <summary>
    /// The <see cref="AlexaOptions"/> to use. This field is read-only.
    /// </summary>
    private readonly AlexaOptions? _options;

    /// <summary>
    /// The <see cref="ILogger"/> to use. This field is read-only.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlexaController"/> class.
    /// </summary>
    /// <param name="userManager">The <see cref="UserManager{TUser}"/> to use.</param>
    /// <param name="telemetry">The <see cref="ISiteTelemetry"/> to use.</param>
    /// <param name="siteOptions">The current site options.</param>
    /// <param name="logger">The <see cref="ILogger"/> to use.</param>
    public AlexaController(
        UserManager<LondonTravelUser> userManager,
        ISiteTelemetry telemetry,
        SiteOptions siteOptions,
        ILogger<AlexaController> logger)
    {
        _userManager = userManager;
        _telemetry = telemetry;
        _options = siteOptions?.Alexa;
        _logger = logger;
    }

    /// <summary>
    /// Generates a new random access token.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing the generated access token.
    /// </returns>
    public static string GenerateAccessToken()
    {
        byte[] entropy = Array.Empty<byte>();

        try
        {
            entropy = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(entropy);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(entropy);
        }
    }

    /// <summary>
    /// Gets the result for the <c>/alexa/authorize/</c> action.
    /// </summary>
    /// <param name="state">The state from the Alexa service.</param>
    /// <param name="clientId">The client Id.</param>
    /// <param name="responseType">The response type.</param>
    /// <param name="redirectUri">The URL to redirect the user to once linked.</param>
    /// <returns>
    /// The result for the <c>/alexa/authorize/</c> action.
    /// </returns>
    [Authorize]
    [HttpGet]
    [Route("authorize", Name = SiteRoutes.AuthorizeAlexa)]
    public async Task<IActionResult> AuthorizeSkill(
        [FromQuery(Name = "state")] string? state,
        [FromQuery(Name = "client_id")] string? clientId,
        [FromQuery(Name = "response_type")] string? responseType,
        [FromQuery(Name = "redirect_uri")] Uri? redirectUri)
    {
        if (_options?.IsLinkingEnabled != true)
        {
            return NotFound();
        }

        if (!VerifyRequest(clientId, responseType, out string? error))
        {
            return RedirectForError(redirectUri, state, error);
        }

        if (!VerifyRedirectUri(redirectUri))
        {
            return BadRequest();
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Log.AlexaLinkFailedUserNotFound(_logger);
                return RedirectForError(redirectUri, state);
            }

            string accessToken = GenerateAccessToken();

            if (!await CreateOrUpdateAccessToken(user, accessToken))
            {
                return RedirectForError(redirectUri, state);
            }

            _telemetry.TrackAlexaLink(user.Id!);

            string tokenRedirectUrl = BuildRedirectUrl(redirectUri!, state, accessToken);
            return Redirect(tokenRedirectUrl);
        }
        catch (Exception ex)
        {
            Log.AlexaLinkFailed(_logger, ex);
            return RedirectForError(redirectUri, state);
        }
    }

    /// <summary>
    /// Builds the redirection URI for the specified parameters.
    /// </summary>
    /// <param name="redirectUri">The base redirection URI.</param>
    /// <param name="state">The value of the state parameter.</param>
    /// <param name="accessToken">The value of the generated access token.</param>
    /// <returns>
    /// The URI to redirect the user to receive the generated access token.
    /// </returns>
    private static string BuildRedirectUrl(Uri redirectUri, string? state, string accessToken)
    {
        var builder = new UriBuilder(redirectUri)
        {
            Fragment = $"state={(state == null ? string.Empty : Uri.EscapeDataString(state))}&access_token={Uri.EscapeDataString(accessToken)}&token_type=Bearer",
        };

        return builder.Uri.AbsoluteUri;
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
        bool hasExistingToken = !string.IsNullOrEmpty(user.AlexaToken);

        if (hasExistingToken)
        {
            Log.RegeneratingAccessToken(_logger, user.Id);
        }
        else
        {
            Log.GeneratingAccessToken(_logger, user.Id);
        }

        user.AlexaToken = accessToken;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            if (hasExistingToken)
            {
                Log.RegeneratedAccessToken(_logger, user.Id);
            }
            else
            {
                Log.GeneratedAccessToken(_logger, user.Id);
            }
        }
        else
        {
            Log.AccessTokenGenerationFailed(
                _logger,
                user.Id,
                string.Join(';', result.Errors.Select((p) => $"{p.Code}: {p.Description}")));
        }

        return result.Succeeded;
    }

    /// <summary>
    /// Returns the redirection for an error for the specified parameters.
    /// </summary>
    /// <param name="redirectUri">The base redirection URI.</param>
    /// <param name="state">The value of the state parameter.</param>
    /// <param name="errorCode">The optional error code.</param>
    /// <returns>
    /// The URI to redirect the user to.
    /// </returns>
    private IActionResult RedirectForError(Uri? redirectUri, string? state, string? errorCode = null)
    {
        string fragment = $"state={(state == null ? string.Empty : Uri.EscapeDataString(state))}&error={errorCode ?? "server_error"}";

        string url;

        if (redirectUri == null)
        {
            url = $"{Url.RouteUrl(SiteRoutes.Home)}#{fragment}";
        }
        else
        {
            var builder = new UriBuilder(redirectUri)
            {
                Fragment = fragment,
            };

            url = builder.Uri.AbsoluteUri;
        }

        return Redirect(url);
    }

    /// <summary>
    /// Verifies the request as-per RFC-6749 section 4.2.2.1.
    /// </summary>
    /// <param name="clientId">The client Id.</param>
    /// <param name="responseType">The response type.</param>
    /// <param name="error">When the method returns, contains the error parameter, if any.</param>
    /// <returns>
    /// <see langword="true"/> if the specified parameter values are valid; otherwise <see langword="false"/>.
    /// </returns>
    private bool VerifyRequest(string? clientId, string? responseType, out string? error)
    {
        error = null;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(responseType))
        {
            error = "invalid_request";
            return false;
        }

        if (!string.Equals(clientId, _options?.ClientId, StringComparison.Ordinal))
        {
            Log.InvalidClientId(_logger, clientId);

            error = "unauthorized_client";
            return false;
        }

        const string ImplicitFlowResponseType = "token";

        if (!string.Equals(responseType, ImplicitFlowResponseType, StringComparison.Ordinal))
        {
            Log.InvalidResponseType(_logger, responseType);

            error = "unsupported_response_type";
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
    private bool VerifyRedirectUri(Uri? redirectUri)
    {
        if (redirectUri == null)
        {
            Log.NoRedirectUri(_logger);
            return false;
        }

        if (!redirectUri.IsAbsoluteUri)
        {
            Log.RedirectUriIsNotAbolute(_logger, redirectUri);
            return false;
        }

        if (_options?.RedirectUrls?.Contains(redirectUri.ToString(), StringComparer.OrdinalIgnoreCase) == false)
        {
            Log.RedirectUriIsNotAuthorized(_logger, redirectUri);
            return false;
        }

        return true;
    }
}
