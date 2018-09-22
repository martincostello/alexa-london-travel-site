// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Identity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// A class that extends <see cref="OAuthEvents"/> to allow for custom error handling.
    /// </summary>
    public class OAuthEventsHandler : OAuthEvents
    {
        /// <summary>
        /// The authentication provider options associated with the instance. This field is read-only.
        /// </summary>
        private readonly OAuthOptions _options;

        /// <summary>
        /// The <see cref="OAuthEvents"/> wrapped by this instance for events it does not handle itself. This field is read-only.
        /// </summary>
        private readonly OAuthEvents _wrapped;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthEventsHandler"/> class.
        /// </summary>
        /// <param name="options">The <see cref="OAuthOptions"/> to use.</param>
        /// <param name="events">The <see cref="ExternalAuthEvents"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        public OAuthEventsHandler(OAuthOptions options, ExternalAuthEvents events, ILoggerFactory loggerFactory)
        {
            _options = options;
            _wrapped = options.Events;
            _logger = loggerFactory.CreateLogger<OAuthEventsHandler>();

            // Setup custom handlers
            OnRemoteFailure = HandleRemoteFailure;
            OnTicketReceived = HandleTicketReceived;

            // Assign delegated handlers
            OnCreatingTicket = _wrapped.CreatingTicket;
            OnRedirectToAuthorizationEndpoint = events?.OnRedirectToOAuthAuthorizationEndpoint ?? _wrapped.RedirectToAuthorizationEndpoint;
        }

        /// <summary>
        /// Handles a remote failure.
        /// </summary>
        /// <typeparam name="T">The type of the secure data.</typeparam>
        /// <param name="context">The failure context.</param>
        /// <param name="provider">The authentication provider.</param>
        /// <param name="secureDataFormat">The secure data format.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        /// <param name="propertiesProvider">A delegate to a method to retrieve authentication properties from the secure data.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        public static Task HandleRemoteFailure<T>(
            RemoteFailureContext context,
            string provider,
            ISecureDataFormat<T> secureDataFormat,
            ILogger logger,
            Func<T, IDictionary<string, string>> propertiesProvider)
        {
            string path = GetSiteErrorRedirect(context, secureDataFormat, propertiesProvider);

            if (string.IsNullOrEmpty(path) ||
                !Uri.TryCreate(path, UriKind.Relative, out Uri notUsed))
            {
                path = "/";
            }

            SiteMessage message;

            if (WasPermissionDenied(context))
            {
                message = SiteMessage.LinkDenied;
                logger.LogTrace("User denied permission.");
            }
            else
            {
                message = SiteMessage.LinkFailed;

                var eventId = default(EventId);
                string errors = string.Join(";", context.Request.Query.Select((p) => $"'{p.Key}' = '{p.Value}'"));
                string logMessage = $"Failed to sign-in using '{provider}': '{context.Failure.Message}'. Errors: {errors}.";

                if (IsCorrelationFailure(context))
                {
                    // Not a server-side problem, so do not create log noise
                    logger.LogTrace(eventId, context.Failure, logMessage);
                }
                else
                {
                    logger.LogError(eventId, context.Failure, logMessage);
                }
            }

            context.Response.Redirect($"{path}?Message={message}");
            context.HandleResponse();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the value of the site context error redirect URL associated with the current request, if any.
        /// </summary>
        /// <typeparam name="T">The type of the secure data.</typeparam>
        /// <param name="context">The failure context.</param>
        /// <param name="secureDataFormat">The secure data format.</param>
        /// <param name="propertiesProvider">A delegate to a method to retrieve authentication properties from the secure data.</param>
        /// <returns>
        /// The site context associated with the current request, if any.
        /// </returns>
        private static string GetSiteErrorRedirect<T>(
            RemoteFailureContext context,
            ISecureDataFormat<T> secureDataFormat,
            Func<T, IDictionary<string, string>> propertiesProvider)
        {
            var state = context.Request.Query["state"];
            var stateData = secureDataFormat.Unprotect(state);
            var properties = propertiesProvider?.Invoke(stateData);

            if (properties == null ||
                !properties.TryGetValue(SiteContext.ErrorRedirectPropertyName, out string value))
            {
                value = null;
            }

            return value;
        }

        /// <summary>
        /// Returns whether the specified failure context indicates that request correlation for XSRF failed.
        /// </summary>
        /// <param name="context">The current failure context.</param>
        /// <returns>
        /// <see langword="true"/> if request correlation failed; otherwise <see langword="false"/>.
        /// </returns>
        private static bool IsCorrelationFailure(RemoteFailureContext context)
        {
            // See https://github.com/aspnet/Security/blob/ad425163b29b1e09a41e84423b0dcbac797c9164/src/Microsoft.AspNetCore.Authentication.OAuth/OAuthHandler.cs#L66
            // and https://github.com/aspnet/Security/blob/2d1c56ce5ccfc15c78dd49cee772f6be473f3ee2/src/Microsoft.AspNetCore.Authentication/RemoteAuthenticationHandler.cs#L203
            // This effectively means that the user did not pass their cookies along correctly to correlate the request.
            return string.Equals(context.Failure.Message, "Correlation failed.", StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns whether the specified failure context indicates the user denied account linking permission.
        /// </summary>
        /// <param name="context">The current failure context.</param>
        /// <returns>
        /// <see langword="true"/> if account linking permission was denied; otherwise <see langword="false"/>.
        /// </returns>
        private static bool WasPermissionDenied(RemoteFailureContext context)
        {
            string error = context.Request.Query["error"].FirstOrDefault();

            if (string.Equals(error, "access_denied", StringComparison.Ordinal) ||
                string.Equals(error, "consent_required", StringComparison.Ordinal))
            {
                return true;
            }

            string reason = context.Request.Query["error_reason"].FirstOrDefault();

            if (string.Equals(reason, "user_denied", StringComparison.Ordinal))
            {
                return true;
            }

            string description = context.Request.Query["error_description"].FirstOrDefault();

            if (!string.IsNullOrEmpty(description) &&
                description.Contains("denied", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return context.Request.Query.ContainsKey("denied");
        }

        /// <summary>
        /// Handles a remote failure.
        /// </summary>
        /// <param name="context">The failure context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        private async Task HandleRemoteFailure(RemoteFailureContext context)
        {
            try
            {
                await HandleRemoteFailure(
                    context,
                    _options.SignInScheme,
                    _options.StateDataFormat,
                    _logger,
                    (p) => p?.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(default, ex, "Failed to handle remote failure: {Message}.", ex.Message);

                if (!context.Result.Handled)
                {
                    await _wrapped.RemoteFailure(context);
                }
            }
        }

        /// <summary>
        /// Handles a ticket being received.
        /// </summary>
        /// <param name="context">The ticket receipt context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        private async Task HandleTicketReceived(TicketReceivedContext context)
        {
            context.Properties.ExpiresUtc = DateTime.UtcNow.AddDays(150);
            context.Properties.IsPersistent = true;

            await _wrapped.TicketReceived(context);
        }
    }
}
