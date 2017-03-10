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
    using Microsoft.AspNetCore.Builder;
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
        /// The <see cref="IOAuthEvents"/> wrapped by this instance for events it does not handle itself. This field is read-only.
        /// </summary>
        private readonly IOAuthEvents _wrapped;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<OAuthEventsHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthEventsHandler"/> class.
        /// </summary>
        /// <param name="options">The <see cref="OAuthOptions"/> to use.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        public OAuthEventsHandler(OAuthOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _wrapped = options.Events;
            _logger = loggerFactory.CreateLogger<OAuthEventsHandler>();

            // Setup custom handlers
            OnRemoteFailure = HandleRemoteFailure;

            // Assign delegated handlers
            OnCreatingTicket = _wrapped.CreatingTicket;
            OnRedirectToAuthorizationEndpoint = _wrapped.RedirectToAuthorizationEndpoint;
            OnTicketReceived = _wrapped.TicketReceived;
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
        internal static Task HandleRemoteFailure<T>(
            FailureContext context,
            string provider,
            ISecureDataFormat<T> secureDataFormat,
            ILogger logger,
            Func<T, IDictionary<string, string>> propertiesProvider)
        {
            string errors = string.Join(";", context.Request.Query.Select((p) => $"'{p.Key}' = '{p.Value}'"));
            logger?.LogError(default(EventId), context.Failure, $"Failed to sign-in using '{provider}': '{context.Failure.Message}'. Errors: {errors}.");

            string path = GetSiteErrorRedirect(context, secureDataFormat, propertiesProvider);

            if (string.IsNullOrEmpty(path) ||
                !Uri.TryCreate(path, UriKind.Relative, out Uri notUsed))
            {
                path = "/";
            }

            SiteMessage message;

            if (string.Equals(context.Request.Query["error"].FirstOrDefault(), "access_denied", StringComparison.Ordinal) ||
                string.Equals(context.Request.Query["error_reason"].FirstOrDefault(), "user_denied", StringComparison.Ordinal) ||
                context.Request.Query.ContainsKey("denied"))
            {
                message = SiteMessage.LinkDenied;
            }
            else
            {
                message = SiteMessage.LinkFailed;
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
            FailureContext context,
            ISecureDataFormat<T> secureDataFormat,
            Func<T, IDictionary<string, string>> propertiesProvider)
        {
            var state = context.Request.Query["state"];
            var stateData = secureDataFormat.Unprotect(state);
            var properties = propertiesProvider?.Invoke(stateData);

            string value;

            if (properties == null ||
                !properties.TryGetValue(SiteContext.ErrorRedirectPropertyName, out value))
            {
                value = null;
            }

            return value;
        }

        /// <summary>
        /// Handles a remote failure.
        /// </summary>
        /// <param name="context">The failure context.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the completion of the operation.
        /// </returns>
        private async Task HandleRemoteFailure(FailureContext context)
        {
            try
            {
                await HandleRemoteFailure(
                    context,
                    _options.AuthenticationScheme,
                    _options.StateDataFormat,
                    _logger,
                    (p) => p?.Items);
            }
            catch (Exception ex)
            {
                _logger?.LogError(default(EventId), ex, $"Failed to handle remote failure: {ex.Message}.");

                if (!context.HandledResponse)
                {
                    await _wrapped.RemoteFailure(context);
                }
            }
        }
    }
}
