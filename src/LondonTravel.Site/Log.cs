// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

//// TODO Cannot use file-scoped namespace due to https://github.com/dotnet/runtime/issues/57880.
//// TODO Once fix available, move log messages to nested Log classes in their calling classes.

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
            EventId = 4,
            Level = LogLevel.Information,
            Message = "Created database {DatabaseName}.")]
        public static partial void CreatedDatabase(ILogger logger, string? databaseName);

        [LoggerMessage(
            EventId = 5,
            Level = LogLevel.Trace,
            Message = "Received API request for user preferences.")]
        public static partial void RequestForPreferences(ILogger logger);

        [LoggerMessage(
            EventId = 6,
            Level = LogLevel.Trace,
            Message = "Creating document in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void CreatingDocument(ILogger logger, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 7,
            Level = LogLevel.Trace,
            Message = "Created document in collection {CollectionName} of database {DatabaseName}. Id: {ResourceId}.")]
        public static partial void CreatedDocument(ILogger logger, string? collectionName, string? databaseName, string? resourceId);

        [LoggerMessage(
            EventId = 8,
            Level = LogLevel.Trace,
            Message = "Querying documents in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void QueryingDocuments(ILogger logger, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 9,
            Level = LogLevel.Trace,
            Message = "Found {DocumentCount:N0} document(s) in collection {CollectionName} of database {DatabaseName} that matched query.")]
        public static partial void QueriedDocuments(ILogger logger, int documentCount, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 10,
            Level = LogLevel.Trace,
            Message = "Replacing document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void ReplacingDocument(ILogger logger, string? resourceId, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 11,
            Level = LogLevel.Trace,
            Message = "Replaced document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName}.")]
        public static partial void ReplacedDocument(ILogger logger, string? resourceId, string? collectionName, string? databaseName);

        [LoggerMessage(
            EventId = 12,
            Level = LogLevel.Warning,
            Message = "Failed to replace document with Id {ResourceId} in collection {CollectionName} of database {DatabaseName} as the write would conflict. ETag: {ETag}.")]
        public static partial void ReplaceFailedWithConflict(ILogger logger, string? resourceId, string? collectionName, string? databaseName, string? etag);

        [LoggerMessage(
            EventId = 13,
            Level = LogLevel.Error,
            Message = "Failed to find user by access token.")]
        public static partial void FailedToFindUserByAccessToken(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = 14,
            Level = LogLevel.Trace,
            Message = "User denied permission.")]
        public static partial void PermissionDenied(ILogger logger);

        [LoggerMessage(
            EventId = 15,
            Level = LogLevel.Trace,
            Message = "Failed to sign-in using {Provider} due to a correlation failure: {FailureMessage}. Errors: {Errors}.")]
        public static partial void CorrelationFailed(ILogger logger, Exception? exception, string provider, string? failureMessage, string errors);

        [LoggerMessage(
            EventId = 16,
            Level = LogLevel.Error,
            Message = "Failed to sign-in using {Provider}: {FailureMessage}. Errors: {Errors}.")]
        public static partial void SignInFailed(ILogger logger, Exception? exception, string provider, string? failureMessage, string errors);

        [LoggerMessage(
            EventId = 17,
            Level = LogLevel.Information,
            Message = "User Id {UserId} signed out.")]
        public static partial void UserSignedOut(ILogger logger, string userId);

        [LoggerMessage(
            EventId = 18,
            Level = LogLevel.Warning,
            Message = "Error from external provider. {RemoteError}")]
        public static partial void RemoteSignInError(ILogger logger, string remoteError);

        [LoggerMessage(
            EventId = 19,
            Level = LogLevel.Information,
            Message = "User Id {UserId} signed in with provider {LoginProvider}.")]
        public static partial void UserSignedIn(ILogger logger, string userId, string loginProvider);

        [LoggerMessage(
            EventId = 20,
            Level = LogLevel.Information,
            Message = "New user account {UserId} created through {LoginProvider}.")]
        public static partial void UserCreated(ILogger logger, string? userId, string loginProvider);

        [LoggerMessage(
            EventId = 21,
            Level = LogLevel.Error,
            Message = "Failed to get user to link account to Alexa.")]
        public static partial void AlexaLinkFailedUserNotFound(ILogger logger);

        [LoggerMessage(
            EventId = 22,
            Level = LogLevel.Error,
            Message = "Failed to link account to Alexa.")]
        public static partial void AlexaLinkFailed(ILogger logger, Exception exception);

        [LoggerMessage(
            EventId = 23,
            Level = LogLevel.Trace,
            Message = "Generating Alexa access token for user Id {UserId}.")]
        public static partial void GeneratingAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 24,
            Level = LogLevel.Trace,
            Message = "Generated Alexa access token for user Id {UserId}.")]
        public static partial void GeneratedAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 25,
            Level = LogLevel.Trace,
            Message = "Regenerating Alexa access token for user Id {UserId}.")]
        public static partial void RegeneratingAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
            EventId = 26,
            Level = LogLevel.Trace,
            Message = "Regenerated Alexa access token for user Id {UserId}.")]
        public static partial void RegeneratedAccessToken(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 27,
           Level = LogLevel.Error,
           Message = "Failed to generate Alexa access token for user Id {UserId}: {Errors}.")]
        public static partial void AccessTokenGenerationFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 28,
           Level = LogLevel.Warning,
           Message = "Invalid client Id {ClientId} specified.")]
        public static partial void InvalidClientId(ILogger logger, string? clientId);

        [LoggerMessage(
           EventId = 29,
           Level = LogLevel.Warning,
           Message = "Invalid response type {ResponseType} specified.")]
        public static partial void InvalidResponseType(ILogger logger, string? responseType);

        [LoggerMessage(
           EventId = 30,
           Level = LogLevel.Warning,
           Message = "No redirection URI specified.")]
        public static partial void NoRedirectUri(ILogger logger);

        [LoggerMessage(
           EventId = 31,
           Level = LogLevel.Warning,
           Message = "The specified redirection URI {RedirectionUri} is not an absolute URI.")]
        public static partial void RedirectUriIsNotAbolute(ILogger logger, Uri redirectionUri);

        [LoggerMessage(
           EventId = 32,
           Level = LogLevel.Warning,
           Message = "The specified redirection URI {RedirectionUri} is not authorized.")]
        public static partial void RedirectUriIsNotAuthorized(ILogger logger, Uri redirectionUri);

        [LoggerMessage(
           EventId = 33,
           Level = LogLevel.Error,
           Message = "Failed to get user to manage account.")]
        public static partial void FailedToGetUserToManageAccount(ILogger logger);

        [LoggerMessage(
           EventId = 34,
           Level = LogLevel.Information,
           Message = "Attempting to link user Id {UserId} to provider {ProviderName}.")]
        public static partial void AttemptingToLinkUser(ILogger logger, string userId, string providerName);

        [LoggerMessage(
           EventId = 35,
           Level = LogLevel.Error,
           Message = "Failed to get external login info for user Id {UserId} to link account.")]
        public static partial void FailedToGetExternalLogin(ILogger logger, string userId);

        [LoggerMessage(
           EventId = 36,
           Level = LogLevel.Trace,
           Message = "Adding login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddingExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
           EventId = 37,
           Level = LogLevel.Information,
           Message = "Added login for provider {ProviderName} to user Id {UserId}.")]
        public static partial void AddedExternalLogin(ILogger logger, string providerName, string userId);

        [LoggerMessage(
           EventId = 38,
           Level = LogLevel.Information,
           Message = "Updated claims for user Id {UserId} for provider {ProviderName}.")]
        public static partial void UpdatedUserClaims(ILogger logger, string userId, string providerName);

        [LoggerMessage(
           EventId = 39,
           Level = LogLevel.Error,
           Message = "Failed to update user Id {UserId} with additional role claims for provider {ProviderName}: {Errors}.")]
        public static partial void UpdatingUserClaimsFailed(ILogger logger, string userId, string providerName, string errors);

        [LoggerMessage(
           EventId = 40,
           Level = LogLevel.Error,
           Message = "Failed to add external login info for user Id {UserId}: {Errors}.")]
        public static partial void AddingExternalLoginFailed(ILogger logger, string userId, string errors);

        [LoggerMessage(
           EventId = 41,
           Level = LogLevel.Trace,
           Message = "Removing login for provider {ProviderName} from user Id {UserId}.")]
        public static partial void RemovingExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
           EventId = 42,
           Level = LogLevel.Information,
           Message = "Removed login for {ProviderName} from user Id {UserId}.")]
        public static partial void RemovedExternalLogin(ILogger logger, string providerName, string? userId);

        [LoggerMessage(
           EventId = 43,
           Level = LogLevel.Error,
           Message = "Failed to remove external login info from user Id {UserId} for provider {ProviderName}: {Errors}.")]
        public static partial void RemovingExternalLoginFailed(ILogger logger, string? userId, string providerName, string errors);

        [LoggerMessage(
           EventId = 44,
           Level = LogLevel.Trace,
           Message = "Removing Alexa link from user Id {UserId}.")]
        public static partial void RemovingAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 45,
           Level = LogLevel.Information,
           Message = "Removed Alexa link from user Id {UserId}.")]
        public static partial void RemovedAlexaLink(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 46,
           Level = LogLevel.Error,
           Message = "Failed to remove Alexa link from user Id {UserId}: {Errors}.")]
        public static partial void RemovingAlexaLinkFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 47,
           Level = LogLevel.Trace,
           Message = "Deleting user Id {UserId}.")]
        public static partial void DeletingUser(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 48,
           Level = LogLevel.Information,
           Message = "Deleted user Id {UserId}.")]
        public static partial void DeletedUser(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 49,
           Level = LogLevel.Error,
           Message = "Failed to delete user Id {UserId}: {Errors}.")]
        public static partial void DeletingUserFailed(ILogger logger, string? userId, string errors);

        [LoggerMessage(
           EventId = 50,
           Level = LogLevel.Error,
           Message = "Failed to get user to delete account.")]
        public static partial void FailedToGetUserToDeleteAccount(ILogger logger);

        [LoggerMessage(
           EventId = 51,
           Level = LogLevel.Error,
           Message = "Failed to get user to update line preferences.")]
        public static partial void FailedToGetUserToUpdateLinePreferences(ILogger logger);

        [LoggerMessage(
           EventId = 52,
           Level = LogLevel.Trace,
           Message = "Updating line preferences for user Id {UserId}.")]
        public static partial void UpdatingLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 53,
           Level = LogLevel.Information,
           Message = "Updated line preferences for user Id {UserId}.")]
        public static partial void UpdatedLinePreferences(ILogger logger, string? userId);

        [LoggerMessage(
           EventId = 54,
           Level = LogLevel.Warning,
           Message = "Failed to update line preferences for user '{UserId}' as it would cause a write conflict. ETag: {ETag}.")]
        public static partial void UpdatingLinePreferencesFailed(ILogger logger, string? userId, string etag);

        [LoggerMessage(
           EventId = 55,
           Level = LogLevel.Information,
           Message = "Created collection {CollectionName} in database {DatabaseName}.")]
        public static partial void CreatedCollection(ILogger logger, string collectionName, string? databaseName);

        [LoggerMessage(
           EventId = 56,
           Level = LogLevel.Error,
           Message = "Failed to get user to render preferences.")]
        public static partial void FailedToGetUser(ILogger logger);

        [LoggerMessage(
           EventId = 57,
           Level = LogLevel.Error,
           Message = "Failed to map TfL lines as there were no values.")]
        public static partial void FailedToMapUserPreferences(ILogger logger);

        [LoggerMessage(
           EventId = 58,
           Level = LogLevel.Warning,
           Message = "{ErrorCode}: {ErrorDescription}")]
        public static partial void IdentityError(ILogger logger, string errorCode, string errorDescription);

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
