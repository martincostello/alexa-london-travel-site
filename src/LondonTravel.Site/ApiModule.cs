// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Headers;
using MartinCostello.LondonTravel.Site.Extensions;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services;
using MartinCostello.LondonTravel.Site.Swagger;
using MartinCostello.LondonTravel.Site.Telemetry;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MartinCostello.LondonTravel.Site;

public static partial class ApiModule
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app, ILogger logger)
    {
        app.MapGet("/api/_count", async (IAccountService service) =>
        {
            long count = await service.GetUserCountAsync(useCache: false);
            return Results.Ok(new { count });
        })
        .ExcludeFromDescription()
        .RequireAuthorization("admin");

        app.MapGet("/api/preferences", async (
            [FromHeader(Name = "Authorization")][SwaggerParameter("The authorization header.")] string? authorizationHeader,
            HttpContext httpContext,
            IAccountService service,
            ISiteTelemetry telemetry,
            CancellationToken cancellationToken) =>
        {
            Log.RequestForPreferences(logger);

            // TODO Consider allowing implicit access if the user is signed-in (i.e. access from a browser)
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                Log.AccessDeniedNoAuthorization(logger, httpContext);
                telemetry.TrackApiPreferencesUnauthorized();

                return Results.Json(Unauthorized(httpContext, "No access token specified."), statusCode: StatusCodes.Status401Unauthorized);
            }

            LondonTravelUser? user = null;
            string? accessToken = GetAccessTokenFromAuthorizationHeader(authorizationHeader, out string? errorDetail);

            if (accessToken != null)
            {
                user = await service.GetUserByAccessTokenAsync(accessToken, cancellationToken);
            }

            if (user == null || !string.Equals(user.AlexaToken, accessToken, StringComparison.Ordinal))
            {
                Log.AccessDeniedUnknownToken(logger, httpContext);
                telemetry.TrackApiPreferencesUnauthorized();

                return Results.Json(Unauthorized(httpContext, "Unauthorized.", errorDetail), statusCode: StatusCodes.Status401Unauthorized);
            }

            Log.AccessAuthorized(logger, user.Id, httpContext);

            var result = new PreferencesResponse()
            {
                FavoriteLines = user.FavoriteLines,
                UserId = user.Id!,
            };

            telemetry.TrackApiPreferencesSuccess(result.UserId);

            return Results.Json(result);
        })
        .Produces<PreferencesResponse, PreferencesResponseExampleProvider>("The preferences associated with the provided access token.")
        .Produces<ErrorResponse, ErrorResponseExampleProvider>("A valid access token was not provided.", StatusCodes.Status401Unauthorized)
        .WithOperationDescription("Gets the preferences for a user associated with an access token.");

        return app;
    }

    private static string? GetAccessTokenFromAuthorizationHeader(string authorizationHeader, out string? errorDetail)
    {
        errorDetail = null;

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out AuthenticationHeaderValue? authorization))
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

    private static ErrorResponse Unauthorized(HttpContext httpContext, string message, string? detail = null)
    {
        return new ErrorResponse()
        {
            Message = message ?? string.Empty,
            RequestId = httpContext.TraceIdentifier,
            StatusCode = StatusCodes.Status401Unauthorized,
            Details = detail == null ? [] : [detail],
        };
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    private static partial class Log
    {
        public static void AccessAuthorized(ILogger logger, string? userId, HttpContext httpContext)
        {
            AccessAuthorized(logger, userId, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers.UserAgent!);
        }

        public static void AccessDeniedNoAuthorization(ILogger logger, HttpContext httpContext)
        {
            AccessDeniedNoAuthorization(logger, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers.UserAgent!);
        }

        public static void AccessDeniedUnknownToken(ILogger logger, HttpContext httpContext)
        {
            AccessDeniedUnknownToken(logger, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers.UserAgent!);
        }

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Trace,
            Message = "Received API request for user preferences.")]
        public static partial void RequestForPreferences(ILogger logger);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Information,
            Message = "Successfully authorized API request for preferences for user Id {UserId}. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessAuthorized(ILogger logger, string? userId, IPAddress? remoteIP, string userAgent);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Information,
            Message = "API request for preferences denied as no Authorization header/value was specified. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessDeniedNoAuthorization(ILogger logger, IPAddress? remoteIP, string userAgent);

        [LoggerMessage(
            EventId = 4,
            Level = LogLevel.Information,
            Message = "API request for preferences denied as the specified access token is unknown. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessDeniedUnknownToken(ILogger logger, IPAddress? remoteIP, string userAgent);
    }
}
