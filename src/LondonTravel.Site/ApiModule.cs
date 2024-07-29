// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using MartinCostello.LondonTravel.Site.Identity;
using MartinCostello.LondonTravel.Site.Models;
using MartinCostello.LondonTravel.Site.Services;
using MartinCostello.OpenApi;
using Microsoft.AspNetCore.Mvc;

namespace MartinCostello.LondonTravel.Site;

public static partial class ApiModule
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/_count", async (IAccountService service) =>
        {
            long count = await service.GetUserCountAsync(useCache: false);
            return Results.Ok(new { count });
        })
        .ExcludeFromDescription()
        .RequireAuthorization("admin");

        app.MapGet("/api/preferences", GetPreferences)
           .WithName("GetApiPreferences")
           .WithSummary("Gets a user's preferences.")
           .WithDescription("Gets the preferences for a user associated with an access token.")
           .WithTags("LondonTravel.Site")
           .Produces<PreferencesResponse>(StatusCodes.Status200OK)
           .Produces<ErrorResponse>(StatusCodes.Status401Unauthorized);

        app.MapGet("/version", static () =>
        {
            return new JsonObject()
            {
                ["applicationVersion"] = GitMetadata.Version,
                ["frameworkDescription"] = RuntimeInformation.FrameworkDescription,
                ["operatingSystem"] = new JsonObject()
                {
                    ["description"] = RuntimeInformation.OSDescription,
                    ["architecture"] = RuntimeInformation.OSArchitecture.ToString(),
                    ["version"] = Environment.OSVersion.VersionString,
                    ["is64Bit"] = Environment.Is64BitOperatingSystem,
                },
                ["process"] = new JsonObject()
                {
                    ["architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
                    ["is64BitProcess"] = Environment.Is64BitProcess,
                    ["isNativeAoT"] = !RuntimeFeature.IsDynamicCodeSupported,
                    ["isPrivilegedProcess"] = Environment.IsPrivilegedProcess,
                },
                ["dotnetVersions"] = new JsonObject()
                {
                    ["runtime"] = GetVersion<object>(),
                    ["aspNetCore"] = GetVersion<HttpContext>(),
                },
            };

            static string GetVersion<T>()
                => typeof(T).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        })
        .AllowAnonymous()
        .ExcludeFromDescription();

        return app;
    }

    [OpenApiResponse(StatusCodes.Status200OK, "The preferences associated with the provided access token.")]
    [OpenApiResponse(StatusCodes.Status401Unauthorized, "A valid access token was not provided.")]
    private static async Task<IResult> GetPreferences(
        [Description("The authorization header.")][FromHeader(Name = "Authorization")] string? authorizationHeader,
        HttpContext httpContext,
        IAccountService service,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("MartinCostello.LondonTravel.Site.ApiModule");

        Log.RequestForPreferences(logger);

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            Log.AccessDeniedNoAuthorization(logger, httpContext);

            return Results.Json(
                Unauthorized(httpContext, "No access token specified."),
                ApplicationJsonSerializerContext.Default.ErrorResponse,
                statusCode: StatusCodes.Status401Unauthorized);
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

            return Results.Json(
                Unauthorized(httpContext, "Unauthorized.", errorDetail),
                ApplicationJsonSerializerContext.Default.ErrorResponse,
                statusCode: StatusCodes.Status401Unauthorized);
        }

        Log.AccessAuthorized(logger, user.Id, httpContext);

        var result = new PreferencesResponse()
        {
            FavoriteLines = user.FavoriteLines,
            UserId = user.Id!,
        };

        return Results.Json(result, ApplicationJsonSerializerContext.Default.PreferencesResponse);
    }

    private static string? GetAccessTokenFromAuthorizationHeader(string authorizationHeader, out string? errorDetail)
    {
        errorDetail = null;

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var authorization))
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
