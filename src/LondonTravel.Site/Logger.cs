// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

//// TODO Cannot use file-scoped namespace due to https://github.com/dotnet/runtime/issues/57880

using System.Net;

namespace MartinCostello.LondonTravel.Site
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static partial class Log
    {
        public static void AccessAuthorized(ILogger logger, string? userId, HttpContext httpContext)
        {
            AccessAuthorized(logger, userId, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers["User-Agent"]);
        }

        public static void AccessDeniedNoAuthorization(ILogger logger, HttpContext httpContext)
        {
            AccessDeniedNoAuthorization(logger, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers["User-Agent"]);
        }

        public static void AccessDeniedUnknownToken(ILogger logger, HttpContext httpContext)
        {
            AccessDeniedUnknownToken(logger, httpContext.Connection.RemoteIpAddress, httpContext.Request.Headers["User-Agent"]);
        }

        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Information,
            Message = "Successfully authorized API request for preferences for user Id {UserId}. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessAuthorized(ILogger logger, string? userId, IPAddress? remoteIP, string userAgent);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Information,
            Message = "API request for preferences denied as no Authorization header/value was specified. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessDeniedNoAuthorization(ILogger logger, IPAddress? remoteIP, string userAgent);

        [LoggerMessage(
            EventId = 3,
            Level = LogLevel.Information,
            Message = "API request for preferences denied as the specified access token is unknown. IP: {RemoteIP}; User Agent: {UserAgent}.")]
        private static partial void AccessDeniedUnknownToken(ILogger logger, IPAddress? remoteIP, string userAgent);
    }
}
