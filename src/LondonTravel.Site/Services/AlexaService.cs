// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Security.Cryptography;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Services;

public sealed class AlexaService
{
    private readonly ILogger _logger;
    private readonly AlexaOptions _options;
    private readonly ISiteTelemetry _telemetry;
    private readonly UserManager<LondonTravelUser> _userManager;

    public AlexaService(
        UserManager<LondonTravelUser> userManager,
        ISiteTelemetry telemetry,
        IOptions<SiteOptions> options,
        ILogger<AlexaService> logger)
    {
        _userManager = userManager;
        _telemetry = telemetry;
        _options = options.Value.Alexa!;
        _logger = logger;
    }

    public static string GenerateAccessToken()
    {
        byte[] entropy = RandomNumberGenerator.GetBytes(64);

        try
        {
            return Convert.ToBase64String(entropy);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(entropy);
        }
    }

    public async Task<IResult> AuthorizeSkillAsync(
        string? state,
        string? clientId,
        string? responseType,
        Uri? redirectUri,
        ClaimsPrincipal user)
    {
        if (_options?.IsLinkingEnabled != true)
        {
            return Results.NotFound();
        }

        if (!VerifyRequest(clientId, responseType, out string? error))
        {
            return RedirectForError(redirectUri, state, error);
        }

        if (!VerifyRedirectUri(redirectUri))
        {
            return Results.BadRequest();
        }

        try
        {
            var travelUser = await _userManager.GetUserAsync(user);

            if (travelUser == null)
            {
                Log.AlexaLinkFailedUserNotFound(_logger);
                return RedirectForError(redirectUri, state);
            }

            string accessToken = GenerateAccessToken();

            if (!await CreateOrUpdateAccessToken(travelUser, accessToken))
            {
                return RedirectForError(redirectUri, state);
            }

            _telemetry.TrackAlexaLink(travelUser.Id!);

            string tokenRedirectUrl = BuildRedirectUrl(redirectUri!, state, accessToken);
            return Results.Redirect(tokenRedirectUrl);
        }
        catch (Exception ex)
        {
            Log.AlexaLinkFailed(_logger, ex);
            return RedirectForError(redirectUri, state);
        }
    }

    private static string BuildRedirectUrl(Uri redirectUri, string? state, string accessToken)
    {
        string stateValue = state == null ? string.Empty : Uri.EscapeDataString(state);
        string tokenValue = Uri.EscapeDataString(accessToken);

        var builder = new UriBuilder(redirectUri)
        {
            Fragment = $"state={stateValue}&access_token={tokenValue}&token_type=Bearer",
        };

        return builder.Uri.AbsoluteUri;
    }

    private static IResult RedirectForError(
        Uri? redirectUri,
        string? state,
        string? errorCode = null)
    {
        string fragment = $"state={(state == null ? string.Empty : Uri.EscapeDataString(state))}&error={errorCode ?? "server_error"}";

        string url;

        if (redirectUri == null)
        {
            url = "/#" + fragment;
        }
        else
        {
            var builder = new UriBuilder(redirectUri)
            {
                Fragment = fragment,
            };

            url = builder.Uri.AbsoluteUri;
        }

        return Results.Redirect(url);
    }

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

    private bool VerifyRequest(string? clientId, string? responseType, out string? error)
    {
        error = null;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(responseType))
        {
            error = "invalid_request";
            return false;
        }

        if (!string.Equals(clientId, _options.ClientId, StringComparison.Ordinal))
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

        if (_options.RedirectUrls?.Contains(redirectUri.ToString(), StringComparer.OrdinalIgnoreCase) == false)
        {
            Log.RedirectUriIsNotAuthorized(_logger, redirectUri);
            return false;
        }

        return true;
    }
}
