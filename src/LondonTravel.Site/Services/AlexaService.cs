// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using System.Security.Cryptography;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MartinCostello.LondonTravel.Site.Services;

public sealed partial class AlexaService(
    UserManager<LondonTravelUser> userManager,
    IOptionsSnapshot<SiteOptions> options,
    ILogger<AlexaService> logger)
{
    private readonly AlexaOptions _options = options.Value.Alexa!;

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
            var travelUser = await userManager.GetUserAsync(user);

            if (travelUser == null)
            {
                Log.AlexaLinkFailedUserNotFound(logger);
                return RedirectForError(redirectUri, state);
            }

            string accessToken = GenerateAccessToken();

            if (!await CreateOrUpdateAccessToken(travelUser, accessToken))
            {
                return RedirectForError(redirectUri, state);
            }

            string tokenRedirectUrl = BuildRedirectUrl(redirectUri!, state, accessToken);
            return Results.Redirect(tokenRedirectUrl);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Log.AlexaLinkFailed(logger, ex);
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
            Log.RegeneratingAccessToken(logger, user.Id);
        }
        else
        {
            Log.GeneratingAccessToken(logger, user.Id);
        }

        user.AlexaToken = accessToken;

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            if (hasExistingToken)
            {
                Log.RegeneratedAccessToken(logger, user.Id);
            }
            else
            {
                Log.GeneratedAccessToken(logger, user.Id);
            }
        }
        else
        {
            Log.AccessTokenGenerationFailed(
                logger,
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
            Log.InvalidClientId(logger, clientId);

            error = "unauthorized_client";
            return false;
        }

        const string ImplicitFlowResponseType = "token";

        if (!string.Equals(responseType, ImplicitFlowResponseType, StringComparison.Ordinal))
        {
            Log.InvalidResponseType(logger, responseType);

            error = "unsupported_response_type";
            return false;
        }

        return true;
    }

    private bool VerifyRedirectUri(Uri? redirectUri)
    {
        if (redirectUri == null)
        {
            Log.NoRedirectUri(logger);
            return false;
        }

        if (!redirectUri.IsAbsoluteUri)
        {
            Log.RedirectUriIsNotAbolute(logger, redirectUri);
            return false;
        }

        if (_options.RedirectUrls?.Contains(redirectUri.ToString(), StringComparer.OrdinalIgnoreCase) == false)
        {
            Log.RedirectUriIsNotAuthorized(logger, redirectUri);
            return false;
        }

        return true;
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Error,
            Message = "Failed to get user to link account to Alexa.")]
        public static partial void AlexaLinkFailedUserNotFound(ILogger logger);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Error,
            Message = "Failed to link account to Alexa.")]
        public static partial void AlexaLinkFailed(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Trace,
            Message = "Generating Alexa access token for user Id {UserId}.")]
        public static partial void GeneratingAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Trace,
            Message = "Generated Alexa access token for user Id {UserId}.")]
        public static partial void GeneratedAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Trace,
            Message = "Regenerating Alexa access token for user Id {UserId}.")]
        public static partial void RegeneratingAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Trace,
            Message = "Regenerated Alexa access token for user Id {UserId}.")]
        public static partial void RegeneratedAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 7,
           Level = LogLevel.Error,
           Message = "Failed to generate Alexa access token for user Id {UserId}: {Errors}.")]
        public static partial void AccessTokenGenerationFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 8,
           Level = LogLevel.Warning,
           Message = "Invalid client Id {ClientId} specified.")]
        public static partial void InvalidClientId(ILogger logger, string? clientId);

        [LoggerMessage(
           EventId = 9,
           Level = LogLevel.Warning,
           Message = "Invalid response type {ResponseType} specified.")]
        public static partial void InvalidResponseType(ILogger logger, string? responseType);

        [LoggerMessage(
           EventId = 10,
           Level = LogLevel.Warning,
           Message = "No redirection URI specified.")]
        public static partial void NoRedirectUri(ILogger logger);

        [LoggerMessage(
           EventId = 11,
           Level = LogLevel.Warning,
           Message = "The specified redirection URI {RedirectionUri} is not an absolute URI.")]
        public static partial void RedirectUriIsNotAbolute(ILogger logger, Uri redirectionUri);

        [LoggerMessage(
           EventId = 12,
           Level = LogLevel.Warning,
           Message = "The specified redirection URI {RedirectionUri} is not authorized.")]
        public static partial void RedirectUriIsNotAuthorized(ILogger logger, Uri redirectionUri);
    }
}
